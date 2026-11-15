const p = (text) => ({ t: 'p', text });
const h1 = (text, first = false) => ({ t: 'h1', text: text.toUpperCase(), first });
const h2 = (text) => ({ t: 'h2', text });
const dash = (items) => [{ t: 'dash', items }];
const num = (items) => [{ t: 'num', items }];
const table = (caption, header, rows, widths) => [{ t: 'table', caption, header, rows, widths }];
const figure = (file, caption, widthCm = 16) => [{ t: 'figure', file, caption, widthCm }];

module.exports = { p, h1, h2, dash, num, table, figure };
