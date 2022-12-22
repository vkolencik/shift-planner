import React, { useEffect, useState } from 'react'
import { addDays, Day, formatDate, getMonth, getStartOfWeek, getWeekDays, toDay } from '../dates'
import { RequirementType, requirementTypes, Shift, useApi, User, UserRequirement } from '../api'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { Table } from 'react-bootstrap'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faBan, faCheck } from '@fortawesome/free-solid-svg-icons'

type DayInfo =
  undefined
  | { status: 'unavailable', reason: RequirementType }
  | { status: 'shift', workplace: string }
  | { status: 'day-after' }

export interface WeekView {[key: DateUserKey]: DayInfo}

export function CreateWeekView (requirements: UserRequirement[], shifts: Shift[] | null): WeekView {
  let view: WeekView = {}

  for (const requirement of requirements) {
    if (['leave', 'hard-no', 'only-afternoon'].includes(requirement.requirementType)) {
      view[GetDayUserKey(requirement.userId, requirement.date)] = {
        status: 'unavailable',
        reason: requirement.requirementType
      }
    }
  }

  if (shifts !== null) {
    for (const shift of shifts) {
      view[GetDayUserKey(shift.user.userId, shift.day)] = { status: 'shift', workplace: shift.workplace }
      view[GetDayUserKey(shift.user.userId, addDays(shift.day, 1))] = { status: 'day-after' }
    }
  }

  return view
}

export type DateUserKey = string & { __isDateUserKey: true }

export function GetDayUserKey (userId: string, date: Day): DateUserKey {
  return `${userId}:${date}` as DateUserKey
}

export const WeeklyOverview: React.FC = () => {
  const [weekStart, setWeekStart] = useState<Day>(addDays(getStartOfWeek(), 30))
  const [shifts, setShifts] = useState<Shift[] | null | undefined>(undefined)
  const [requirements, setRequirements] = useState<UserRequirement[] | undefined>(undefined)
  const [users, setUsers] = useState<User[] | undefined>(undefined)
  const api = useApi()

  useEffect(() => {
    const month = getMonth(weekStart)
    api.requirements.getMonth(month)
      .then(setRequirements)
    api.schedule.get(month)
      .then(s => setShifts(s?.shifts))
    api.users.get()
      .then(setUsers)
  }, [weekStart])

  const weekDays = getWeekDays(weekStart)

  if (requirements === undefined || shifts === undefined || users === undefined) {
    return <LoadingSpinner/>
  }

  const weeklyView = CreateWeekView(requirements, shifts)

  function DayCell (props: { day: string, userId: string }) {
    const dayInfo = weeklyView[GetDayUserKey(props.userId, toDay(props.day))]

    if (dayInfo === undefined) {
      return <td className="table-success text-center" title="K dispozici"><FontAwesomeIcon icon={faCheck}/></td>
    }

    switch (dayInfo.status) {
      case 'unavailable':
        return <td className="table-danger text-center" title={requirementTypes[dayInfo.reason].name}>
          <FontAwesomeIcon icon={faBan}/>
        </td>
      case 'shift':
        return <td className="table-danger text-center" title="Služba"><strong>{dayInfo.workplace}</strong></td>
      case 'day-after':
        return <td className="table-danger text-center" title='Po službě'>
          <FontAwesomeIcon icon={faBan}/>
        </td>
      default:
        throw new Error('Unknown day info status')
    }
  }

  return <div className="container">
    <Table>
      <thead>
      <th scope="col"></th>
      {weekDays.map(d => <th scope="col" className='text-center'>{formatDate(d)}</th>)}
      </thead>
      <tbody>
      {users.map(u =>
        <tr>
          <th scope="row">{u.userName}</th>
          {weekDays.map(d => <DayCell day={d} userId={u.userId}/>)}
        </tr>
      )}
      </tbody>
    </Table>
  </div>
}
