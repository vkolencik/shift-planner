import React, { useEffect, useState } from 'react'
import { MonthRequirements, RequirementType, useApi, User } from '../service/api'
import {
  dateToString,
  getCurrentMonth,
  getNumberOfDaysOfMonth,
  isWeekend,
  parseDate
} from '../dates'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { Table } from 'react-bootstrap'
import {Printer} from 'react-bootstrap-icons'
import './MonthlyOverview.css'
import { useUserList } from '../service/users'

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
  const users = useUserList()
  const [requirements, setRequirements] = useState<MonthRequirements | null>(null)

  const month = getCurrentMonth()
  useEffect(() => {
    if (requirements === null) {
      api.requirements.getMonth(month).then(setRequirements)
    }
  }, [])

  if (requirements == null || users == null) {
    return <LoadingSpinner/>
  }

  const daysInMonth = getNumberOfDaysOfMonth(month)
  const days = new Array(daysInMonth).fill(1).map((_, i) => i + 1)

  const dailySums = new Array(daysInMonth).fill(users.length - 6)

  const requirementsPerUserDay: { [key: string]: string } = {}
  const countsAsAbsence = (r: RequirementType) => r === 'leave' || r === 'hard-no' || r === 'only-afternoon'
  for (const requirement of requirements) {
    const day = parseDate(requirement.date).getDate()
    if (countsAsAbsence(requirement.requirementType)) {
      dailySums[day - 1]--
    }
    requirementsPerUserDay[`${requirement.userId}-${day}`] = requirement.requirementType
  }

  const currentMonth = getCurrentMonth()
  const weekends: number[] = days.filter(day => isWeekend(dateToString(currentMonth.year, currentMonth.month, day)))

  return <div className='big-table'>
    <button className='btn btn-primary no-print' onClick={window.print}>
      <Printer/>&nbsp;Tisk
    </button>
    <Table bordered hover className='table-fit table-sm table-bordered'>
      <thead className='thead-light sticky-top table-primary table-bordered'>
      <tr>
        <th>Datum</th>
        {days.map(day => weekends.includes(day)
          ? <th scope='col' className='weekend-day text-center'>{day}</th>
          : <th scope='col' className='text-center'>{day}</th>)}
      </tr>
      <tr>
        <th>Lidí k dispozici</th>
        {days.map(day => weekends.includes(day)
          ? <th scope="col" className="weekend-day">&nbsp;</th>
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
