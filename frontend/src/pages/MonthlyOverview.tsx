import React, { useEffect, useState } from 'react'
import { MonthRequirements, useApi, User } from '../requirements/api'
import { currentMonth, currentYear, getNumberOfDaysOfCurrentMonth, isWeekend, parseDate } from '../dates'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { Table } from 'react-bootstrap'
import { sortArrayBy } from '../sort-array-by'
import './MonthlyOverview.css'

export const MonthlyOverview: React.FC = () => {
  const api = useApi()
  const [users, setUsers] = useState<User[] | null>(null)
  const [requirements, setRequirements] = useState<MonthRequirements | null>(null)

  useEffect(() => {
    if (users == null) {
      api.users.get().then(users => {
        sortArrayBy(users, u => u.userName)
        setUsers(users)
      })
    }

  }, [])

  useEffect(() => {
    if (requirements == null) {
      api.requirements.getMonth(currentYear, currentMonth).then(setRequirements)
    }
  }, [])

  if (requirements == null || users == null) {
    return <LoadingSpinner/>
  }

  const daysInMonth = getNumberOfDaysOfCurrentMonth()
  const days = new Array(daysInMonth).fill(1).map((_, i) => i + 1)

  const dailySums = new Array(daysInMonth).fill(users.length - 6)
  const absences = requirements.filter(r => r.requirementType === 'leave' || r.requirementType === 'hard-no')
  const absenceKeys: string[] = []
  for (const requirement of absences) {
    const date = parseDate(requirement.date)
    const day = date.getDate()
    dailySums[day - 1]--
    absenceKeys.push(`${requirement.userId}-${day}`)
  }

  const weekends: number[] = days.filter(day => isWeekend(new Date(currentYear, currentMonth - 1, day)))


  return <div style={{ position: 'fixed', height: '100%', overflow: 'scroll' }}>
    <Table bordered hover className="table-fit table-sm">
      <thead className="thead-light sticky-top table-primary">
      <tr>
        <th>Datum</th>
        {days.map(day => weekends.includes(day)
          ? <th scope="col" className="weekend-day">{day}</th>
          : <th scope="col">{day}</th>)}
      </tr>
      <tr>
        <th>Lidí k dispozici</th>
        {days.map(day => weekends.includes(day)
          ? <th scope="col" className="weekend-day"></th>
          : <th scope="col">{dailySums[day - 1]}</th>)}
      </tr>
      </thead>
      <tbody>
      {users.map(user =>
        <tr>
          <th scope="row">{user.userName}</th>
          {days.map(day => weekends.includes(day)
            ? <td className="weekend-day"></td>
            : <td className={absenceKeys.includes(`${user.userId}-${day}`) ? 'table-danger' : ''}></td>)}
        </tr>
      )}
      </tbody>
    </Table>
  </div>
}
