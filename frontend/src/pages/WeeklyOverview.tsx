import React, { useEffect, useState } from 'react'
import {
  addDays,
  compareMonths,
  Day,
  formatDate,
  getDayOfWeekNumber,
  getMonth,
  getStartOfWeek,
  getWeekDays,
  toDay
} from '../dates'
import { requiredAvailabilitiesSummer2023 } from './daily-required-availabilities-summer-2023'
import { RequirementType, requirementTypes, ShiftAssignment, useApi, UserRequirement, UserWAD } from '../service/api'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { Alert, Button, ButtonGroup, Table } from 'react-bootstrap'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faArrowLeft, faArrowRight, faBan, faCheck } from '@fortawesome/free-solid-svg-icons'
import { useUserList } from '../service/users'
import { Permission, usePermissions } from '../auth/auth'
import { isExternalUser } from '../users'

type DayInfo =
  undefined
  | { status: 'unavailable', reason: RequirementType }
  | { status: 'shift', workplace: string }
  | { status: 'day-after' }
  | { status: 'inactive' }

export interface WeekView {[key: DateUserKey]: DayInfo}

export function CreateWeekView (requirements: UserRequirement[], shifts: ShiftAssignment[] | null): WeekView {
  let view: WeekView = {}

  for (const requirement of requirements) {
    if (['leave', 'hard-no', 'not-working', 'only-afternoon'].includes(requirement.requirementType)) {
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
      if (compareMonths(getMonth(shift.day), {year: 2023, month:12}) === 0) {
        view[getDayUserKey(shift.user.userId, addDays(shift.day, 2))] = { status: 'day-after' }
      }
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
  const allUsers = useUserList()
  const api = useApi()
  const permissions = usePermissions()

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

  if (loadingWeek || allUsers === undefined) {
    return <LoadingSpinner/>
  }

  const users = allUsers.filter(u => compareMonths(u.activeUntil, getMonth(weekStart)) >= 0)

  const weeklyView = CreateWeekView(requirements, shifts)

  function getDayInfo(user: UserWAD, day: Day): DayInfo {
    return compareMonths(user.activeUntil, getMonth(day)) < 0 ? { status: 'inactive' } : weeklyView[getDayUserKey(user.userId, day)]
  }

  function DayCell (props: { day: Day, userId: string }) {
    const user = users.find(u => u.userId === props.userId)

    if (user === undefined) {
      throw new Error(`Cannot find user ${props.userId}`)
    }

    const dayInfo = getDayInfo(user, props.day)

    if (dayInfo === undefined) {
      return <td className="table-success text-center" title="K dispozici"><FontAwesomeIcon icon={faCheck}/></td>
    }

    switch (dayInfo.status) {
      case 'unavailable':
        return <td className="table-danger text-center" title={requirementTypes[dayInfo.reason].name}>
          <FontAwesomeIcon icon={faBan}/>
        </td>
      case 'inactive':
        return <td className="table-danger text-center" title='Uživatel deaktivován'>
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
    let available = users.filter(u => !isExternalUser(u)).length

    for (const user of users) {
      const dayInfo = getDayInfo(user, day)
      if (dayInfo !== undefined && (dayInfo.status === 'day-after' || dayInfo.status === 'unavailable' || dayInfo.status === 'inactive')) {
        available--
      }
    }

    dailyAvailabilities.push(available)
  }

  const getRequiredAvailability = (day: Day): number | null => {
    if (toDay('2023-07-01') <= day && day <= toDay('2023-08-31')) {
      return requiredAvailabilitiesSummer2023[day]
    }

    if (toDay('2023-09-01') <= day && day <= toDay('2023-09-30')) {
      const requiredAvailabilitiesSeptember2023 = [25, 25, 26, 25, 22, null, null]
      return requiredAvailabilitiesSeptember2023[getDayOfWeekNumber(day) - 1]
    }

    if (toDay('2023-10-01') <= day && day <= toDay('2023-10-31')) {
      const requiredAvailabilitiesOctober = [25, 25, 27, 26, 22, null, null]
      return requiredAvailabilitiesOctober[getDayOfWeekNumber(day) - 1]
    }

    const requiredAvailabilities = [26, 26, 27, 25, 22, null, null]
    return requiredAvailabilities[getDayOfWeekNumber(day) - 1]
  }

  return <div className="container">
    <ButtonGroup>
      <Button variant="outline-secondary" onClick={handlePreviousWeek}><FontAwesomeIcon icon={faArrowLeft}/></Button>
      <Button variant="outline-secondary" onClick={handleCurrentWeek}>Aktuální týden</Button>
      <Button variant="outline-secondary" onClick={handleNextWeek}><FontAwesomeIcon icon={faArrowRight}/></Button>
    </ButtonGroup>
    {someShiftDataMissing && <Alert variant="warning">Pro toto období zatím nebyl vygenerován rozvrh služeb.</Alert>}
    <Table>
      <thead className="thead-light table-primary">
      <tr>
        <th scope="col"></th>
        {weekDays.map(d => <th scope="col" className="text-center">{formatDate(d)}</th>)}
      </tr>
      <tr>
        <th scope="row">K dispozici</th>
        {dailyAvailabilities.map(a => <th scope="col" className="text-center">{a}</th>)}
      </tr>
      <tr>
        <th scope="row">Potřebný počet</th>
        {weekDays.map(d => <th scope="col" className="text-center">{getRequiredAvailability(d)}</th>)}
      </tr>
      </thead>
      <tbody>
      {users.map((u, i) =>
        <tr key={u.userId}>
          <th scope="row" style={{width: '1%'}}>{u.userName}</th>
          {permissions.includes(Permission.readRequirementsOfOthers)
            ? weekDays.map(d => <DayCell day={d} userId={u.userId}/>)
            : (i === 0 ? <td colSpan={7} rowSpan={users.length} className="table-secondary"><span style={{position: 'sticky', left: '20px', top: '50vh'}}>Tento uživatel nemá k dispozici náhled</span></td> : <></>)}
        </tr>
      )}
      </tbody>
    </Table>
  </div>
}
