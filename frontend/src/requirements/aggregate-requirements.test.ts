import { aggregateRequirements } from './aggregate-requirements'
import { MonthRequirements } from '../api'

describe('Requirements aggregation should', () => {
  it('return empty object when there are no leaves', () => {
    const requirements: MonthRequirements = [
      { userId: 'user2', userName: 'User 2', date: '2022-11-30', requirementType: 'hard-no' },
      { userId: 'user1', userName: 'User 1', date: '2022-11-27', requirementType: 'hard-no' },
      { userId: 'user1', userName: 'User 1', date: '2022-11-29', requirementType: 'hard-no' },
      { userId: 'user1', userName: 'User 1', date: '2022-11-28', requirementType: 'hard-no' },
    ]

    const aggregatedLeaves = aggregateRequirements(requirements, false)

    expect(aggregatedLeaves).toEqual({})
  })

  it('aggregate by user and filter leaves', () => {
    const requirements: MonthRequirements = [
      { userId: 'user2', userName: 'User 2', date: '2022-11-30', requirementType: 'leave', approved: false },
      { userId: 'user1', userName: 'User 1', date: '2022-11-27', requirementType: 'leave', approved: false },
      { userId: 'user1', userName: 'User 1', date: '2022-11-29', requirementType: 'leave', approved: true },
      { userId: 'user1', userName: 'User 1', date: '2022-11-28', requirementType: 'hard-no' },
    ]

    const aggregatedLeaves = aggregateRequirements(requirements, false)

    expect(aggregatedLeaves).toEqual({
      'User 1': [
        { day: '2022-11-27', approved: false },
        { day: '2022-11-29', approved: true }
      ],
      'User 2': [
        { day: '2022-11-30', approved: false },
      ]
    })
  })

  it('hide approved', () => {
    const requirements: MonthRequirements = [
      { userId: 'user2', userName: 'User 2', date: '2022-11-29', requirementType: 'leave', approved: true },
      { userId: 'user2', userName: 'User 2', date: '2022-11-30', requirementType: 'leave', approved: false },
    ]

    const aggregatedLeaves = aggregateRequirements(requirements, true)

    expect(aggregatedLeaves).toEqual({
      'User 2': [
        { day: '2022-11-30', approved: false },
      ]
    })

  })
})

export {}
