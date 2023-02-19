import { CreateWeekView, DateUserKey, getDayUserKey, WeekView } from './WeeklyOverview'
import { ShiftAssignment, User, UserRequirement } from '../service/api'
import { toDay } from '../dates'

describe('Weekly overview should', () => {
  it('generate user-day key', () => {
    const key = getDayUserKey('user1', toDay('2022-12-22'))
    expect(key).toEqual('user1:2022-12-22')
  })

  it('calculate week view', () => {
    const requirements: UserRequirement[] = [
      { userId: 'user1', userName: 'User 1', requirementType: 'leave', date: toDay('2022-12-01'), approved: true },
      { userId: 'user1', userName: 'User 1', requirementType: 'hard-no', date: toDay('2022-12-02'), approved: true },
      { userId: 'user1', userName: 'User 1', requirementType: 'only-afternoon', date: toDay('2022-12-03'), approved: true },
      { userId: 'user1', userName: 'User 1', requirementType: 'only-morning', date: toDay('2022-12-04'), approved: true },
      { userId: 'user1', userName: 'User 1', requirementType: 'soft-yes', date: toDay('2022-12-05'), approved: true },
      { userId: 'user1', userName: 'User 1', requirementType: 'hard-no', date: toDay('2022-12-22'), approved: true },
      { userId: 'user2', userName: 'User 2', requirementType: 'hard-no', date: toDay('2022-12-15'), approved: true },
    ]
    const shifts: ShiftAssignment[] = [
      { day: toDay('2022-12-01'), workplace: 'X11', user: { userId: 'user2', userName: 'User 2' } },
      { day: toDay('2022-12-21'), workplace: 'ARO', user: { userId: 'user1', userName: 'User 1' } },
    ]

    const view = CreateWeekView(requirements, shifts)

    const expectedView: WeekView = {
      ['user1:2022-12-01' as DateUserKey]: {status: 'unavailable', reason: 'leave'},
      ['user1:2022-12-02' as DateUserKey]: {status: 'unavailable', reason: 'hard-no'},
      ['user1:2022-12-03' as DateUserKey]: {status: 'unavailable', reason: 'only-afternoon'},
      ['user2:2022-12-15' as DateUserKey]: {status: 'unavailable', reason: 'hard-no'},
      ['user2:2022-12-01' as DateUserKey]: {status: 'shift', workplace: 'X11'},
      ['user2:2022-12-02' as DateUserKey]: {status: 'day-after'},
      ['user1:2022-12-21' as DateUserKey]: {status: 'shift', workplace: 'ARO'},
      ['user1:2022-12-22' as DateUserKey]: {status: 'day-after'},
    }

    expect(view).toEqual(expectedView)
  })

  it('Prioritize Day-after before requirements', () => {
    const requirements: UserRequirement[] = [
      { userId: 'user1', userName: 'User 1', requirementType: 'leave', date: toDay('2022-12-02'), approved: true }
    ]

    const shifts: ShiftAssignment[] = [
      { day: toDay('2022-12-01'), workplace: 'X11', user: { userId: 'user1', userName: 'User 1' } }
    ]

    const view = CreateWeekView(requirements, shifts)

    expect(view).toEqual(expect.objectContaining({
      ['user1:2022-12-02' as DateUserKey]: { status: 'day-after' }
    }))
  })
})
