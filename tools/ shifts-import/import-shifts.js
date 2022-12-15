const fs = require('fs')

const rawShifts = JSON.parse(fs.readFileSync('shifts.json'))

const workplaces = [
  'A1',
  'A2',
  'A3',
  'RES',
  'KPR',
  'URG'
]

const users = JSON.parse(fs.readFileSync('users.json')).Result

let shiftUsers = []
for (const day of rawShifts) {
  shiftUsers.push(...day.shifts.map(s => s.name))
}

shiftUsers = [... new Set(shiftUsers)]

const userNames = users.map(u => u.UserName);
const notFoundUsers = shiftUsers.filter(name => userNames.indexOf(name) < 0)
console.log(notFoundUsers)

const shifts = rawShifts.map(s => ({
  date: s.date,

  shifts: [0, 1, 2, 3, 4, 5].map(i => ({
    workplace: workplaces[i],
    userId: users.find(u => u.UserName === s.shifts[i].name).UserId
  }))
}))

console.log(rawShifts)

fs.writeFileSync('./output.json', JSON.stringify(shifts))
