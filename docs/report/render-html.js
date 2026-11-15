const IMG_BASE = 'https://raw.githubusercontent.com/University-Work-xXx-GloriousPhoenix-xXx/cmdes-coursework/feature/code/docs/report/';

const esc = (s) => s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
const inline = (s) => esc(s)
  .replace(/\*\*(.+?)\*\*/g, '<b>$1</b>')
  .replace(/_\{(.+?)\}/g, '<sub>$1</sub>')
  .replace(/\^\{(.+?)\}/g, '<sup>$1</sup>');

const F = 'font-family:\'Times New Roman\';font-size:14pt;line-height:150%;margin:0;';
const BODY = `${F}text-align:justify;text-indent:1.25cm;`;
const CELL = 'font-family:\'Times New Roman\';font-size:12pt;line-height:100%;margin:0;';

function block(b) {
  switch (b.t) {
    case 'p': return `<p style="${BODY}">${inline(b.text)}</p>`;
    case 'h1': return `<h1 style="${F}text-align:center;font-weight:bold;margin-bottom:12pt;${b.first ? '' : 'page-break-before:always;'}">${inline(b.text)}</h1>`;
    case 'h2': return `<h2 style="${F}text-indent:1.25cm;font-weight:bold;margin-top:12pt;margin-bottom:6pt;">${inline(b.text)}</h2>`;
    case 'dash': return b.items.map((x) => `<p style="${BODY}">–&nbsp;${inline(x)}</p>`).join('\n');
    case 'num': return b.items.map((x, i) => `<p style="${BODY}">${i + 1})&nbsp;${inline(x)}</p>`).join('\n');
    case 'table': {
      const total = b.widths.reduce((a, c) => a + c, 0);
      const td = (x, i, th) => `<td style="border:1px solid #000;padding:2pt 4pt;width:${(b.widths[i] * 100 / total).toFixed(0)}%;"><p style="${CELL}${th ? 'text-align:center;' : ''}">${inline(x)}</p></td>`;
      return `<p style="${F}text-align:justify;margin-top:6pt;">${inline(b.caption)}</p>
<table style="border-collapse:collapse;width:100%;">
<tr>${b.header.map((x, i) => td(x, i, true)).join('')}</tr>
${b.rows.map((r) => `<tr>${r.map((x, i) => td(x, i, false)).join('')}</tr>`).join('\n')}
</table>
<p style="${BODY}"></p>`;
    }
    case 'figure': {
      const px = Math.round(b.widthCm / 2.54 * 96);
      return `<p style="${F}text-align:center;margin-top:6pt;"><img src="${IMG_BASE}${b.file}" width="${px}"></p>
<p style="${F}text-align:center;margin-bottom:12pt;">${inline(b.caption)}</p>`;
    }
  }
}

module.exports = (blocks) => `<html><head><meta charset="utf-8"></head><body>\n${blocks.map(block).join('\n')}\n</body></html>\n`;
