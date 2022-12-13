import React, { useEffect, useState } from 'react'
import { MonthRequirements, RequirementType, useApi, User } from '../requirements/api'
import { currentMonth, currentYear, getNumberOfDaysOfCurrentMonth, isWeekend, parseDate } from '../dates'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { Table } from 'react-bootstrap'
import { sortArrayBy } from '../sort-array-by'
import './MonthlyOverview.css'

type CellDetails = { cssClass: string, titleText: string, cellText: string }
const absenceClasses: { [requirementType in RequirementType]: CellDetails } = {
  'leave': {
    cssClass: 'table-purple',
    cellText: 'D',
    titleText: 'Dovolená',
  },
  'soft-yes': {
    cssClass: 'table-success',
    cellText: 'Ch',
    titleText: 'Chci'
  },
  'hard-no': {
    cssClass: 'table-danger',
    cellText: 'N',
    titleText: 'Nemůžu'
  },
  'only-morning': {
    cssClass: 'table-warning',
    cellText: 'R',
    titleText: 'Pouze R'
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
  return <td className={`${details?.cssClass} text-center`} title={details?.titleText}>
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

  const requirementsPerUserDay: { [key: string]: string } = {}
  const countsAsAbsence = (r: RequirementType) => r === 'leave' || r === 'hard-no' || r == 'only-afternoon'
  for (const requirement of requirements) {
    const day = parseDate(requirement.date).getDate()
    if (countsAsAbsence(requirement.requirementType)) {
      dailySums[day - 1]--
    }
    requirementsPerUserDay[`${requirement.userId}-${day}`] = requirement.requirementType
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
          {days.map(day => <DayCell requirementType={requirementsPerUserDay[`${user.userId}-${day}`]}/>
          )}
        </tr>
      )}
      </tbody>
    </Table>
  </div>
}
