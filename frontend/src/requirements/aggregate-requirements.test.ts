import { aggregateRequirements } from './aggregate-requirements'
import { MonthRequirements } from './requirements-service'

describe('Requirements aggregation should', () => {
  it('return empty object when there are no leaves', () => {
    const requirements: MonthRequirements = [
      { userId: 'user2', userName: 'User 2', date: '2022-11-30', requirementType: 'hard-no' },
      { userId: 'user1', userName: 'User 1', date: '2022-11-27', requirementType: 'hard-no' },
      { userId: 'user1', userName: 'User 1', date: '2022-11-29', requirementType: 'hard-no' },
      { userId: 'user1', userName: 'User 1', date: '2022-11-28', requirementType: 'hard-no' },
    ]

    const aggregatedLeaves = aggregateRequirements(requirements)

    expect(aggregatedLeaves).toEqual({})
  })

  it('aggregate by user and filter leaves', () => {
    const requirements: MonthRequirements = [
      { userId: 'user2', userName: 'User 2', date: '2022-11-30', requirementType: 'leave' },
      { userId: 'user1', userName: 'User 1', date: '2022-11-27', requirementType: 'leave' },
      { userId: 'user1', userName: 'User 1', date: '2022-11-29', requirementType: 'leave' },
      { userId: 'user1', userName: 'User 1', date: '2022-11-28', requirementType: 'hard-no' },
    ]

    const aggregatedLeaves = aggregateRequirements(requirements)

    expect(aggregatedLeaves).toEqual({
      'User 1': [
        { day: '2022-11-27' },
        { day: '2022-11-29' }
      ],
      'User 2': [
        { day: '2022-11-30' },
      ]
    })
  })
})

export {}
