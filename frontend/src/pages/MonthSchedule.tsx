import React, { ReactNode, useEffect, useState } from 'react'
import { Schedule, useApi } from '../api'
import {
  formatDate,
  getCurrentMonth,
  getCurrentMonthString, getDayOfWeek,
  getMonthDates,
  isWeekend,
  Month
} from '../dates'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { Alert, Table } from 'react-bootstrap'

export function MonthSchedule (): JSX.Element {
  const [schedule, setSchedule] = useState<Schedule | null | undefined>(undefined)
  const [month, setMonth] = useState<Month>(getCurrentMonth)
  const api = useApi()

  useEffect(() => {
    if (schedule === undefined) {
      api.schedule.get(month)
        .then(setSchedule)
    }
  }, [schedule, month])

  if (schedule === undefined) {
    return <LoadingSpinner/>
  }

  if (schedule === null) {
    return <Alert variant="warning">Pro tento měsíc zatím nejsou vygenerované služby</Alert>
  }

  const workplaces = [
    'A1',
    'A2',
    'A3',
    'RES',
    'KPR',
    'URG'
  ]

  return <>
    <h1>{getCurrentMonthString()}</h1>
    <Table bordered hover className="table-fit table-sm table-bordered">
      <thead className="thead-light sticky-top table-primary table-bordered">
      <tr>
        <th></th>
        {workplaces.map(w => <th scope='col'>{w}</th>)}
      </tr>
      </thead>
      <tbody>
      {getMonthDates(month).map(d =>
        (<tr className={isWeekend(d) ? 'table-success' : ''}>
          <th scope='row'>{formatDate(d)} ({getDayOfWeek(d)})</th>
          {workplaces.map(w => <td>
            {schedule?.shifts?.find(s => s.day === d && s.workplace === w)?.user.userName}
          </td>)}
        </tr>)
      )}
      </tbody>
    </Table>
  </>
}
