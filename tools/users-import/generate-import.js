const fs = require('fs')
const path = require('path')

const importData = 'Tester Testovič\te2e-test-user-1@krtek.me'

const generateImport = importString => importString
  .split('\n')
  .filter(_ => _.trim().length > 0 && _.indexOf('\t') > 0)
  .map(_ => _.split('\t'))
  .map(([name, email]) => ({
    name,
    email,
    email_verified: true
  }))

fs.writeFile(
  path.join(__dirname, 'imports', `import-${new Date().toISOString().split('T')[0]}.json`),
  JSON.stringify(generateImport(importData)),
  {encoding: 'utf8'},
  err => {
    if (err) {
      console.error(err)
    }
  })
