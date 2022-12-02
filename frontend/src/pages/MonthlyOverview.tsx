import React, { useEffect, useState } from 'react'
import { MonthRequirements, useApi, User } from '../requirements/api'
import { currentMonth, currentYear, getDay, getNumberOfDaysOfCurrentMonth } from '../dates'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { Table } from 'react-bootstrap'
import { sortArrayBy } from '../sort-array-by'

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

  const dailySums = new Array(daysInMonth).fill(users.length)

  const absences = requirements.filter(r => r.requirementType === 'leave' || r.requirementType === 'hard-no')
  const absenceKeys: string[] = []
  for (const requirement of absences) {
    const day = getDay(requirement.date)
    dailySums[day - 1]--
    absenceKeys.push(`${requirement.userId}-${day}`)
  }

  return <div style={{position: 'fixed', height: '100%', overflow: 'scroll'}}>
    <Table bordered hover className="table-fit table-sm">
      <thead className="thead-light sticky-top table-light">
      <tr>
        <th>Datum</th>
        {days.map(day => <th scope='col'>{day}</th>)}
      </tr>
      <tr>
        <th>Lidí k dispozici</th>
        {days.map(day => <th scope='col'>{dailySums[day - 1]}</th>)}
      </tr>
      </thead>
      <tbody>
      {users.map(user =>
        <tr>
          <th scope='row'>{user.userName}</th>
          {days.map(day => <td className={absenceKeys.includes(`${user.userId}-${day}`) ? 'table-danger' : ''}></td>)}
        </tr>
      )}
      </tbody>
    </Table>
  </div>
}
