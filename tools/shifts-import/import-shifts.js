const fs = require('fs')

const rawShifts = JSON.parse(fs.readFileSync('shifts2.json'))

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
