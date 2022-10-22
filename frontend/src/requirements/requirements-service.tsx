export const requirementTypes: { [key in RequirementType]: { name: string, value: RequirementType } } = {
  'leave': { name: 'Dovolená', value: 'leave' },
  'soft-yes': { name: 'Chci', value: 'soft-yes' },
  'hard-no': { name: 'Nemůžu', value: 'hard-no' },
  'only-morning': { name: 'Pouze R', value: 'only-morning' },
  'only-afternoon': { name: 'Až odpoledne', value: 'only-afternoon' }
}

export type RequirementType = 'leave' | 'hard-no' | 'soft-yes' | 'only-morning' | 'only-afternoon'

export interface Requirement {
  id: string
  date: string
  type: RequirementType
}

export type RequirementCreationCommand = Omit<Requirement, 'id'>

let id = 0
const requirements: Requirement[] = [
  {
    id: '' + id++,
    type: 'soft-yes',
    date: '2022-12-06'
  },
  {
    id: '' + id++,
    type: 'soft-yes',
    date: '2022-12-08'
  },
  {
    id: '' + id++,
    type: 'leave',
    date: '2022-12-21',
  },
  {
    id: '' + id++,
    type: 'hard-no',
    date: '2022-12-13'
  }
]

export function getRequirements(user: any): Requirement[] {
  return requirements
}

export function addRequirement(user: any, requirement: RequirementCreationCommand): Requirement {
  const createdRequirement = { ...requirement, id: '' + id++}
  requirements.push(createdRequirement)
  return createdRequirement
}
