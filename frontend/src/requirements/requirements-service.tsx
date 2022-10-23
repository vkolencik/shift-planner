export const requirementTypes: { [key in RequirementType]: { name: string, value: RequirementType } } = {
  'leave': { name: 'Dovolená', value: 'leave' },
  'soft-yes': { name: 'Chci', value: 'soft-yes' },
  'hard-no': { name: 'Nemůžu', value: 'hard-no' },
  'only-morning': { name: 'Pouze R', value: 'only-morning' },
  'only-afternoon': { name: 'Až odpoledne', value: 'only-afternoon' }
}

export type RequirementType = 'leave' | 'hard-no' | 'soft-yes' | 'only-morning' | 'only-afternoon'

export type EmployeeMonthRequirements = { [key: string]: RequirementType }
const requirements: EmployeeMonthRequirements = {
  '2022-12-06': 'soft-yes',
  '2022-12-08': 'soft-yes',
  '2022-12-21': 'leave',
  '2022-12-13': 'hard-no'
}

export function getRequirements(user: any): EmployeeMonthRequirements {
  return {...requirements}
}

export function addRequirement(user: any, date: string, type: RequirementType) {
  requirements[date] = type
}

export function removeRequirement(user: any, date: string) {
  delete requirements[date]
}
