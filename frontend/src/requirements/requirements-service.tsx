export const requirementTypes: { [key: string]: { name: string, value: string } } = {
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

export function getRequirements(user: any): Requirement[] {
  return [
    {
      id: '1',
      type: 'soft-yes',
      date: '2022-12-06'
    },
    {
      id: '2',
      type: 'soft-yes',
      date: '2022-12-08'
    },
    {
      id: '3',
      type: 'leave',
      date: '2022-12-21',
    },
    {
      id: '4',
      type: 'hard-no',
      date: '2022-12-13'
    }
  ]
}
