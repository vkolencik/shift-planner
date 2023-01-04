import React, { useEffect, useState } from 'react'
import { addDays, Day, formatDate, getMonth, getStartOfWeek, getWeekDays, toDay } from '../dates'
import { RequirementType, requirementTypes, ShiftAssignment, useApi, User, UserRequirement } from '../service/api'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { Alert, Button, ButtonGroup, Table } from 'react-bootstrap'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faBan, faCheck, faArrowLeft, faArrowRight } from '@fortawesome/free-solid-svg-icons'
import { useUserList } from '../service/users'

type DayInfo =
  undefined
  | { status: 'unavailable', reason: RequirementType }
  | { status: 'shift', workplace: string }
  | { status: 'day-after' }

export interface WeekView {[key: DateUserKey]: DayInfo}

export function CreateWeekView (requirements: UserRequirement[], shifts: ShiftAssignment[] | null): WeekView {
  let view: WeekView = {}

  for (const requirement of requirements) {
    if (['leave', 'hard-no', 'only-afternoon'].includes(requirement.requirementType)) {
      view[getDayUserKey(requirement.userId, requirement.date)] = {
        status: 'unavailable',
        reason: requirement.requirementType
      }
    }
  }

  if (shifts !== null) {
    for (const shift of shifts) {
      view[getDayUserKey(shift.user.userId, shift.day)] = { status: 'shift', workplace: shift.workplace }
      view[getDayUserKey(shift.user.userId, addDays(shift.day, 1))] = { status: 'day-after' }
    }
  }

  return view
}

export type DateUserKey = string & { __isDateUserKey: true }

export function getDayUserKey (userId: string, date: Day): DateUserKey {
  return `${userId}:${date}` as DateUserKey
}

export const WeeklyOverview: React.FC = () => {
  const [weekStart, setWeekStart] = useState<Day>(getStartOfWeek())
  const [shifts, setShifts] = useState<ShiftAssignment[]>([])
  const [requirements, setRequirements] = useState<UserRequirement[]>([])
  const [someShiftDataMissing, setSomeShiftDataMissing] = useState<boolean>(false)
  const [loadingWeek, setLoadingWeek] = useState<boolean>(true)
  const users = useUserList()
  const api = useApi()

  useEffect(() => {
    setRequirements([])
    setShifts([])
    setSomeShiftDataMissing(false)
    setLoadingWeek(true)

    const monthWeekStart = getMonth(addDays(weekStart, -1))
    const monthWeekEnd = getMonth(addDays(weekStart, 6))
    const months = [monthWeekStart]

    if (monthWeekEnd.month !== monthWeekStart.month) {
      months.push(monthWeekEnd)
    }

    const promises = [
      ...months.map(month =>
        api.requirements.getMonth(month)
          .then(reqs => setRequirements(x => [...x, ...reqs]))
      ),
      ...months.map(month =>
        api.schedule.get(month)
          .then(s => {
            if (s === null) {
              setSomeShiftDataMissing(true)
            } else {
              setShifts(x => [...x, ...s.shifts])
            }
          })
      )]

    Promise.all(promises).then(() => setLoadingWeek(false))
  }, [weekStart])

  const weekDays = getWeekDays(weekStart)

  if (loadingWeek || users === undefined) {
    return <LoadingSpinner/>
  }

  const weeklyView = CreateWeekView(requirements, shifts)

  function DayCell (props: { day: string, userId: string }) {
    const dayInfo = weeklyView[getDayUserKey(props.userId, toDay(props.day))]

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
        return <td className="table-danger text-center" title="Po službě">
          <FontAwesomeIcon icon={faBan}/>
        </td>
      default:
        throw new Error('Unknown day info status')
    }
  }

  function handlePreviousWeek () {
    setWeekStart(addDays(weekStart, -7))
  }

  function handleCurrentWeek () {
    setWeekStart(getStartOfWeek())
  }

  function handleNextWeek () {
    setWeekStart(addDays(weekStart, 7))
  }

  const dailyAvailabilities = []
  for (const day of weekDays) {
    let available = users.length

    for (const user of users) {
      if (weeklyView[getDayUserKey(user.userId, day)] !== undefined){
        available--
      }
    }

    dailyAvailabilities.push(available)
  }

  return <div className="container">
    <ButtonGroup>
      <Button variant='outline-secondary' onClick={handlePreviousWeek}><FontAwesomeIcon icon={faArrowLeft}/></Button>
      <Button variant='outline-secondary' onClick={handleCurrentWeek}>Aktuální týden</Button>
      <Button variant='outline-secondary' onClick={handleNextWeek}><FontAwesomeIcon icon={faArrowRight}/></Button>
    </ButtonGroup>
    {someShiftDataMissing && <Alert variant="warning">Pro toto období zatím nebyl vygenerován rozvrh služeb.</Alert>}
    <Table>
      <thead className='thead-light table-primary'>
      <tr>
        <th scope="col"></th>
        {weekDays.map(d => <th scope="col" className="text-center">{formatDate(d)}</th>)}
      </tr>
      <tr>
        <th scope="row">K dispozici</th>
        {dailyAvailabilities.map(a => <th scope="col" className="text-center">{a}</th>)}
      </tr>
      </thead>
      <tbody>
      {users.map(u =>
        <tr key={u.userId}>
          <th scope="row">{u.userName}</th>
          {weekDays.map(d => <DayCell day={d} userId={u.userId}/>)}
        </tr>
      )}
      </tbody>
    </Table>
  </div>
}
