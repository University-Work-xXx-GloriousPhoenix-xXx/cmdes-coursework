const fs = require('fs');
const path = require('path');
const d = require('docx');

const FONT = 'Times New Roman';
const PT = 28;          // half-points for 14 pt
const LINE = 360;       // 1.5 line spacing
const INDENT = 709;     // 1.25 cm
const CONTENT_W = 9638; // A4 width minus 2 cm margins, DXA

function runs(text, base = {}) {
  // **bold**, _{sub}, ^{sup}
  const out = [];
  const re = /\*\*(.+?)\*\*|_\{(.+?)\}|\^\{(.+?)\}/g;
  let last = 0, m;
  while ((m = re.exec(text))) {
    if (m.index > last) out.push(new d.TextRun({ text: text.slice(last, m.index), ...base }));
    if (m[1]) out.push(new d.TextRun({ text: m[1], bold: true, ...base }));
    if (m[2]) out.push(new d.TextRun({ text: m[2], subScript: true, ...base }));
    if (m[3]) out.push(new d.TextRun({ text: m[3], superScript: true, ...base }));
    last = re.lastIndex;
  }
  if (last < text.length) out.push(new d.TextRun({ text: text.slice(last), ...base }));
  return out;
}

const P = (text) => new d.Paragraph({ children: runs(text), style: 'Body' });

const H1 = (text, first = false) => new d.Paragraph({
  heading: d.HeadingLevel.HEADING_1, pageBreakBefore: !first,
  children: [new d.TextRun(text)],
});

const H2 = (text) => new d.Paragraph({ heading: d.HeadingLevel.HEADING_2, children: [new d.TextRun(text)] });

const dash = (items) => items.map((t, i) => new d.Paragraph({
  children: runs(t), style: 'Body', numbering: { reference: 'dash', level: 0 },
}));

let numInstance = 0;
const num = (items) => {
  const inst = ++numInstance;
  return items.map((t) => new d.Paragraph({
    children: runs(t), style: 'Body', numbering: { reference: 'num', level: 0, instance: inst },
  }));
};

function table(caption, header, rows, widths) {
  const total = widths.reduce((a, b) => a + b, 0);
  const w = widths.map((x) => Math.round(x * CONTENT_W / total));
  w[w.length - 1] += CONTENT_W - w.reduce((a, b) => a + b, 0);
  const border = { style: d.BorderStyle.SINGLE, size: 4, color: '000000' };
  const borders = { top: border, bottom: border, left: border, right: border };
  const cell = (text, i, bold, keep) => new d.TableCell({
    width: { size: w[i], type: d.WidthType.DXA }, borders,
    margins: { top: 40, bottom: 40, left: 80, right: 80 },
    children: [new d.Paragraph({ style: 'Cell', keepNext: keep, alignment: bold ? d.AlignmentType.CENTER : d.AlignmentType.LEFT, children: runs(text, bold ? { bold: false } : {}) })],
  });
  return [
    new d.Paragraph({ style: 'Caption', keepNext: true, children: runs(caption) }),
    new d.Table({
      width: { size: CONTENT_W, type: d.WidthType.DXA }, columnWidths: w,
      rows: [
        new d.TableRow({ tableHeader: true, children: header.map((h, i) => cell(h, i, true, true)) }),
        ...rows.map((r, j) => new d.TableRow({ cantSplit: true, children: r.map((c, i) => cell(c, i, false, rows.length <= 8 && j < rows.length - 1)) })),
      ],
    }),
    new d.Paragraph({ style: 'Body', children: [] }),
  ];
}

function figure(file, caption, widthCm = 16) {
  const buf = fs.readFileSync(file);
  const wPx = buf.readUInt32BE(16), hPx = buf.readUInt32BE(20);
  const width = Math.round(widthCm / 2.54 * 96);
  const height = Math.round(width * hPx / wPx);
  return [
    new d.Paragraph({ alignment: d.AlignmentType.CENTER, keepNext: true, spacing: { before: 120, line: 240, lineRule: d.LineRuleType.AUTO },
      children: [new d.ImageRun({ type: 'png', data: buf, transformation: { width, height } })] }),
    new d.Paragraph({ style: 'FigCaption', children: runs(caption) }),
  ];
}

function build(children, out) {
  const doc = new d.Document({
    styles: {
      default: { document: { run: { font: FONT, size: PT }, paragraph: { spacing: { line: LINE } } } },
      paragraphStyles: [
        { id: 'Body', name: 'Body', basedOn: 'Normal', quickFormat: true,
          run: { font: FONT, size: PT },
          paragraph: { alignment: d.AlignmentType.JUSTIFIED, indent: { firstLine: INDENT }, spacing: { line: LINE, before: 0, after: 0 } } },
        { id: 'Heading1', name: 'Heading 1', basedOn: 'Normal', next: 'Body', quickFormat: true,
          run: { font: FONT, size: PT, bold: true },
          paragraph: { alignment: d.AlignmentType.CENTER, spacing: { line: LINE, before: 0, after: 240 }, keepNext: true, outlineLevel: 0 } },
        { id: 'Heading2', name: 'Heading 2', basedOn: 'Normal', next: 'Body', quickFormat: true,
          run: { font: FONT, size: PT, bold: true },
          paragraph: { alignment: d.AlignmentType.LEFT, indent: { firstLine: INDENT }, spacing: { line: LINE, before: 240, after: 120 }, keepNext: true, outlineLevel: 1 } },
        { id: 'Caption', name: 'Table Caption', basedOn: 'Normal',
          run: { font: FONT, size: PT },
          paragraph: { alignment: d.AlignmentType.JUSTIFIED, spacing: { line: LINE, before: 120, after: 0 }, keepNext: true } },
        { id: 'FigCaption', name: 'Figure Caption', basedOn: 'Normal', next: 'Body',
          run: { font: FONT, size: PT },
          paragraph: { alignment: d.AlignmentType.CENTER, spacing: { line: LINE, before: 60, after: 240 } } },
        { id: 'Cell', name: 'Table Cell', basedOn: 'Normal',
          run: { font: FONT, size: 24 },
          paragraph: { spacing: { line: 240, before: 0, after: 0 } } },
      ],
    },
    numbering: { config: [
      { reference: 'dash', levels: [{ level: 0, format: d.LevelFormat.BULLET, text: '–', alignment: d.AlignmentType.LEFT,
        style: { paragraph: { indent: { left: INDENT + 360, hanging: 360 } } } }] },
      { reference: 'num', levels: [{ level: 0, format: d.LevelFormat.DECIMAL, text: '%1)', alignment: d.AlignmentType.LEFT,
        style: { paragraph: { indent: { left: INDENT + 400, hanging: 400 } } } }] },
    ] },
    sections: [{
      properties: { page: { size: { width: 11906, height: 16838 }, margin: { top: 1134, bottom: 1134, left: 1134, right: 1134, header: 567 } } },
      headers: { default: new d.Header({ children: [new d.Paragraph({ alignment: d.AlignmentType.RIGHT,
        children: [new d.TextRun({ children: [d.PageNumber.CURRENT], font: FONT, size: PT })] })] }) },
      children,
    }],
  });
  return d.Packer.toBuffer(doc).then((b) => fs.writeFileSync(out, b));
}

function toDocx(b) {
  switch (b.t) {
    case 'p': return [P(b.text)];
    case 'h1': return [H1(b.text, b.first)];
    case 'h2': return [H2(b.text)];
    case 'dash': return dash(b.items);
    case 'num': return num(b.items);
    case 'table': return table(b.caption, b.header, b.rows, b.widths);
    case 'figure': return figure(b.file, b.caption, b.widthCm);
  }
}

module.exports = (blocks, out) => build(blocks.flatMap(toDocx), out);
