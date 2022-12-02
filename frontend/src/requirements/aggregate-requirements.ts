import { MonthRequirements } from './api'
import { sortArrayBy } from '../sort-array-by'

interface Leave {
  day: string,
  approved: boolean
}

interface LeavesByUser { [userName: string]: Leave[]}

export function aggregateRequirements (requirements: MonthRequirements, hideApproved: boolean): LeavesByUser {
  let leaves = requirements.filter(r => r.requirementType === 'leave')

  if (hideApproved) {
    leaves = leaves.filter(l => l.approved !== true)
  }

  sortArrayBy(leaves, l => l.date)

  const userIds = Array.from(new Set(leaves.map(r => r.userId)))

  const entries = userIds.map(uid => [
    leaves.find(l => l.userId === uid)!.userName,
    leaves.filter(l => l.userId === uid).map(l => ({
      day: l.date,
      approved: l.approved === true
    }))]
  )

  sortArrayBy(entries, e => e[0])

  return Object.fromEntries(entries)
}

