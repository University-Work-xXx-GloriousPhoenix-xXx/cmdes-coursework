const IMG_BASE = 'https://raw.githubusercontent.com/University-Work-xXx-GloriousPhoenix-xXx/cmdes-coursework/feature/code/docs/report/';

const esc = (s) => s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
const inline = (s) => esc(s)
  .replace(/\*\*(.+?)\*\*/g, '<b>$1</b>')
  .replace(/_\{(.+?)\}/g, '<sub>$1</sub>')
  .replace(/\^\{(.+?)\}/g, '<sup>$1</sup>');

const STYLE = `*{font-family:'Times New Roman';font-size:14pt;line-height:150%;margin:0}
p.b{text-align:justify;text-indent:1.25cm}
h1{font-size:14pt;text-align:center;font-weight:bold;margin-bottom:12pt}
h2{font-size:14pt;text-indent:1.25cm;font-weight:bold;margin-top:12pt;margin-bottom:6pt}
p.tc{text-align:justify;margin-top:6pt}
p.c{text-align:center}
p.fc{text-align:center;margin-bottom:12pt}
td{border:1px solid #000;padding:2pt 4pt;vertical-align:top}
td p{font-size:12pt;line-height:100%}
td p.h{text-align:center}
table{border-collapse:collapse;width:100%}`;

function block(b) {
  switch (b.t) {
    case 'p': return `<p class="b">${inline(b.text)}</p>`;
    case 'h1': return `<h1>${inline(b.text)}</h1>`;
    case 'h2': return `<h2>${inline(b.text)}</h2>`;
    case 'dash': return b.items.map((x) => `<p class="b">–&nbsp;${inline(x)}</p>`).join('\n');
    case 'num': return b.items.map((x, i) => `<p class="b">${i + 1})&nbsp;${inline(x)}</p>`).join('\n');
    case 'table': {
      const total = b.widths.reduce((a, c) => a + c, 0);
      const td = (x, i, h) => `<td width="${Math.round(b.widths[i] * 100 / total)}%"><p${h ? ' class="h"' : ''}>${inline(x)}</p></td>`;
      return `<p class="tc">${inline(b.caption)}</p>
<table>
<tr>${b.header.map((x, i) => td(x, i, true)).join('')}</tr>
${b.rows.map((r) => `<tr>${r.map((x, i) => td(x, i, false)).join('')}</tr>`).join('\n')}
</table>
<p class="b"></p>`;
    }
    case 'figure':
      return `<p class="c"><img src="${IMG_BASE}${b.file}" width="${Math.round(b.widthCm / 2.54 * 96)}"></p>
<p class="fc">${inline(b.caption)}</p>`;
  }
}

module.exports = (blocks) => `<html><head><meta charset="utf-8"><style>${STYLE}</style></head><body>
${blocks.map(block).join('\n')}
</body></html>
`;
