import { Moment } from 'moment'
import React, { useEffect, useState } from 'react'
import { formatDate, formatDateMoment, getMonth, getStartOfWeek, getWeekDays } from '../dates'
import { EmployeeMonthRequirements, MonthRequirements, Schedule, Shift, useApi, User, UserRequirement } from '../api'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { Table } from 'react-bootstrap'

export const WeeklyOverview: React.FC = () => {
  const [weekStart, setWeekStart] = useState<Moment>(getStartOfWeek().add(1, 'month'))
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

  const weekDays = getWeekDays(weekStart).map(d => d.format('yyyy-MM-DD'))

  if (requirements === undefined || shifts === undefined || users === undefined) {
    return <LoadingSpinner/>
  }

  function DayCell (props: { day: string, userId: string }) {
    const shift = shifts?.find(s => s.day === props.day && s.user.userId === props.userId)
    if (shift !== undefined) {
      return <td>{shift.workplace}</td>
    }

    if (requirements === undefined) {
      return <td></td>
    }

    const requirement = requirements.find(r => r.userId === props.userId && r.date === props.day)

    return <td>{requirement?.requirementType}</td>
  }

  return <div className="container">
    <Table>
      <thead>
      <th scope="col"></th>
      {weekDays.map(d => <th scope="col">{formatDate(d)}</th>)}
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
