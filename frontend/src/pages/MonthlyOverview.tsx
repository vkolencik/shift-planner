import React, { useEffect, useState } from 'react'
import { MonthRequirements, RequirementType, useApi, User } from '../requirements/api'
import { currentMonth, currentYear, getNumberOfDaysOfCurrentMonth, isWeekend, parseDate } from '../dates'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { Table } from 'react-bootstrap'
import { sortArrayBy } from '../sort-array-by'
import './MonthlyOverview.css'

type CellDetails = { cssClass: string, titleText: string, cellText: string }
const absenceClasses: Partial<{ [requirementType in RequirementType]: CellDetails }> = {
  'hard-no': {
    cssClass: 'table-danger',
    cellText: 'N',
    titleText: 'Nemůžu'
  },
  'leave': {
    cssClass: 'table-purple',
    cellText: 'D',
    titleText: 'Dovolená',
  },
  'only-afternoon': {
    cssClass: 'table-primary',
    cellText: 'O',
    titleText: 'Jen odpoledne'
  }
}

function DayCell ({ requirementType }: { requirementType: string | undefined }) {
  let details: CellDetails | null = null
  if (requirementType !== undefined && requirementType in absenceClasses) {
    // @ts-ignore
    details = absenceClasses[requirementType]
  }

  // @ts-ignore
  return <td className={details?.cssClass} title={details?.titleText}>
    {details?.cellText}
  </td>
}

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
  const absences = requirements.filter(r => r.requirementType === 'leave' || r.requirementType === 'hard-no' || r.requirementType == 'only-afternoon')
  const dayTypes: { [key: string]: string } = {}
  for (const requirement of absences) {
    const date = parseDate(requirement.date)
    const day = date.getDate()
    dailySums[day - 1]--
    dayTypes[`${requirement.userId}-${day}`] = requirement.requirementType
  }

  const weekends: number[] = days.filter(day => isWeekend(new Date(currentYear, currentMonth - 1, day)))

  return <div style={{ position: 'fixed', height: '90%', overflow: 'scroll' }}>
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
            : <DayCell requirementType={dayTypes[`${user.userId}-${day}`]}/>
          )}
        </tr>
      )}
      </tbody>
    </Table>
  </div>
}
