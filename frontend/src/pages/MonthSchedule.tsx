import React, { useEffect, useState } from 'react'
import { Schedule, useApi } from '../service/api'
import {
  formatDate,
  getCurrentMonth,
  getDayOfWeek,
  getMonthDates, getMonthLabel,
  isWeekend,
  Month
} from '../dates'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { Alert, Tab, Table, Tabs } from 'react-bootstrap'
import { sortArrayBy } from '../sort-array-by'

export function MonthSchedule (): JSX.Element {
  const [schedule, setSchedule] = useState<Schedule | null | undefined>(undefined)
  const [month, setMonth] = useState<Month>({month: 1, year: 2023})
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
    return <Alert variant='warning'>Pro tento měsíc zatím nejsou vygenerované služby</Alert>
  }

  const workplaces = [
    'A1',
    'A2',
    'A3',
    'RES',
    'KPR',
    'URG'
  ]

  const userShifts: { [name: string]: number } = {}
  for (const shift of schedule.shifts) {
    const userName = shift.user.userName
    if (!userShifts[userName]) {
      userShifts[userName] = 0
    }

    userShifts[userName]++
  }
  const userShiftEntries = Object.entries(userShifts)
  sortArrayBy(userShiftEntries, e => e[0])

  return <>
    <h1>{getMonthLabel(month)}</h1>
    <Tabs
      defaultActiveKey='schedule'
      id='uncontrolled-tab-example'
      className='mb-3'
    >
      <Tab eventKey='schedule' title='Rozvrh' style={{overflowY:'scroll'}}>

          <Table bordered hover className='table-fit table-sm table-bordered'>
            <thead className='thead-light table-primary table-bordered'>
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
      </Tab>
      <Tab eventKey='shiftCounts' title='Počty služeb'>
        <Table bordered hover className='table-fit table-sm '>
          <tbody>
          {userShiftEntries.map(([userName, shifts]) =>
            (<tr>
              <th scope='row'>{userName}</th>
              <td>{shifts}</td>
            </tr>)
          )}
          </tbody>
        </Table>
      </Tab>
    </Tabs>
  </>
}
