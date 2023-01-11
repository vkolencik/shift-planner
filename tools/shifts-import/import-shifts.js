const fs = require('fs')

const table = 'Bohm\tBeranová\tSepp\tŠumanová\tStrnadelová\tVobořil\n' +
  'Lubbadová\tKorchová\tPaulovičová\tJánský\tDžunda\tPolka\n' +
  'Dohnalová\tStepaniuk\tMiřátská\tŠraierová\tDrašnar\tŠebele\n' +
  'Touficová\tSedlačková\tDžbánková\tFedurčák\tŠumanová\tKoutová\n' +
  'Dohnalová\tStepaniuk\tMiřátská\tŠraierová\tŠebele\tDrašnar\n' +
  'Zlámal\tMarešová\tProcklová\tNedvěd\tStrnadelová\tVobořil\n' +
  'Touficová\tDžunda\tPaulovičová\tJánský\tBohm\tSedlačková\n' +
  'Jiřička\tFedurčák\tSepp\tTesařík\tSoučková\tŠumanová\n' +
  'Zlámal\tStepaniuk\tMiřátská\tŠraierová\tDrašnar\tFischer\n' +
  'Strnadelová\tDžunda\tPaulovičová\tNedvěd\tVobořil\tMarešová\n' +
  'Lubbadová\tHobzíková\tProcklová\tJánský\tSoučková\tPolka\n' +
  'Strnadelová\tMarešová\tPaulovičová\tNedvěd\tVobořil\tDžunda\n' +
  'Drašnar\tBeranová\tMiřátská\tMichalovič\tKoutová\tJagerská\n' +
  'Bohm\tSedlačková\tProcklová\tŠraierová\tHobzíková\tFedurčák\n' +
  'Strnadelová\tMarešová\tSepp\tJánský\tVobořil\tDžbánková\n' +
  'Jiřička\tFischer\tPolka\tNedvěd\tMichalovič\tDžunda\n' +
  'Zlámal\tKorchová\tSedlačková\tŠumanová\tFedurčák\tJagerská\n' +
  'Drašnar\tBeranová\tSepp\tBohm\tStepaniuk\tŠebele\n' +
  'Zlámal\tKorchová\tSedlačková\tŠumanová\tJagerská\tFedurčák\n' +
  'Touficová\tPolka\tMiřátská\tNedvěd\tDohnalová\tVobořil\n' +
  'Hobzíková\tFischer\tPaulovičová\tBala\tMichalovič\tKoutová\n' +
  'Jiřička\tBeranová\tMarešová\tTesařík\tSoučková\tFedurčák\n' +
  'Lubbadová\tKorchová\tDžbánková\tNedvěd\tJagerská\tStepaniuk\n' +
  'Koutová\tFischer\tProcklová\tMichalovič\tSoučková\tBala\n' +
  'Strnadelová\tMarešová\tMiřátská\tJánský\tVobořil\tDžbánková\n' +
  'Koutová\tFischer\tProcklová\tMichalovič\tSoučková\tBala\n' +
  'Zlámal\tKorchová\tPaulovičová\tTesařík\tJagerská\tPolka\n' +
  'Šebele\tStepaniuk\tDžbánková\tŠumanová\tVobořil\tSedlačková'

const rawShifts = table
  .split('\n')
  .map((dayShifts, i) => (
    {
      date: `2023-02-${i + 1 >= 10 ? '' : '0'}${i + 1}`,
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
    shifts.push({
      Day: day.date,
      Workplace: workplaces[i],
      UserId: users.find(u => u.userName === day.shifts[i].name).userId
    })
  }
}

fs.writeFileSync('./output.json', JSON.stringify({ Shifts: shifts }))
