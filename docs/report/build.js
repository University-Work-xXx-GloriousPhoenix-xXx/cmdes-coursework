const fs = require('fs');
const blocks = [
  ...require('./s1')(),
];
const out = process.argv[2] || 'report';
fs.writeFileSync(`${out}.html`, require('./render-html')(blocks));
require('./render-docx')(blocks, `${out}.docx`).then(() => console.log('ok'));
