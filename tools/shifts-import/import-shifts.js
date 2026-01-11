const fs = require('fs')

const table = 'Rottová\tDžunda\tTesařová\tMichalovič\t\tMarešová\n' +
  'Strnadelová\tStepaniuk\tProcklová\tBohm\t\tSedlačková\n' +
  'Rottová\tDžunda\tTesařová\tMichalovič\t\tDrašnar\n' +
  'Hobzíková\tFedurčák\tZvara\tŠebele\t\tExternista - Černý\n' +
  'Zlámal\tPolka\tKlíč\tŠraierová\tŠkola\tExternista - chirurg\n' +
  'Bohm\tStepaniuk\tTesařová\tTesařík\t\tSedlačková\n' +
  'Dohnalová\tBeranová\tMelnik\tMichalovič\t\tDžunda\n' +
  'Fedurčák\tPolka\tPaulovičová\tJánský\t\tMarešová\n' +
  'Lubbadová\tDrašnar\tZvara\tŠebele\t\tExternista - Bilska\n' +
  'Fedurčák\tPolka\tPaulovičová\tJánský\t\tMarešová\n' +
  'Zlámal\tBeranová\tKlíč\tTesařík\t\tExternista - Černý\n' +
  'Rottová\tŠraierová\tProcklová\tŠkola\t\tExternista - chirurg\n' +
  'Dohnalová\tPolka\tZvara\tŠebele\t\tSepp\n' +
  'Jiřička\tMichalovič\tBrablecová\tJánský\t\tDžbánková\n' +
  'Rottová\tKrupková\tMiřátská\tŠraierová\t\tZyková\n' +
  'Drašnar\tKorchová\tKlíč\tŠkola\t\tSepp\n' +
  'Rottová\tKrupková\tMiřátská\tŠraierová\t\tZyková\n' +
  'Zlámal\tMichalovič\tMelnik\tExternista - Pařízek\t\tExternista - Černý\n' +
  'Šebele\tDžunda\tZvara\tŠkola\t\tExternista - chirurg\n' +
  'Jiřička\tPolka\tSepp\tExternista - Pařízek\t\tZyková\n' +
  'Michalovič\tKrupková\tMelnik\tJánský\t\tDžbánková\n' +
  'Strnadelová\tStepaniuk\tProcklová\tExternista - Pařízek\t\tPaulovičová\n' +
  'Dohnalová\tPolka\tTesařová\tŠebele\t\tMarešová\n' +
  'Strnadelová\tStepaniuk\tProcklová\tJánský\t\tPaulovičová\n' +
  'Zlámal\tKorchová\tMelnik\tBohm\t\tDžbánková\n' +
  'Dohnalová\tLubbadová\tMiřátská\tRottová\t\tExternista - chirurg\n' +
  'Jiřička\tKrupková\tBrablecová\tTesařík\t\tExternista - Bilska\n' +
  'Rottová\tKorchová\tMarešová\tJánský\t\tZyková\n' +
  'Zlámal\tSedlačková\tMelnik\tŠraierová\t\tExternista - Černý\n' +
  'Jiřička\tBeranová\tBrablecová\tTesařík\t\tDžbánková\n' +
  'Zlámal\tStepaniuk\tMelnik\tŠkola\tSedlačková\tSepp'

const rawShifts = table
  .split('\n')
  .map((dayShifts, i) => (
    {
      date: `2023-12-${i + 1 >= 10 ? '' : '0'}${i + 1}`,
      shifts: dayShifts.split('\t').map(name => ({name: name}))
    }))

const workplaces = [
  'A1',
  'A2',
  'A3',
  'RES',
  'KPR',
  'URG'
]

const users = JSON.parse(fs.readFileSync('./users.json'))

let shiftUsers = []
for (const day of rawShifts) {
  shiftUsers.push(...day.shifts.map(s => s.name))
}

shiftUsers = [...new Set(shiftUsers)]

const userNames = users.map(u => u.userName)
const notFoundUsers = shiftUsers.filter(name => userNames.indexOf(name) < 0)
console.log(notFoundUsers)

const shifts = []

for (const day of rawShifts) {
  for (let i = 0; i < workplaces.length; i++) {

    if (day.shifts[i].name === '') continue

    shifts.push({
      Day: day.date,
      Workplace: workplaces[i],
      UserId: users.find(u => u.userName === day.shifts[i].name).userId
    })
  }
}

fs.writeFileSync('./2023-11-14-shifts-import.json', JSON.stringify({ Shifts: shifts }))
