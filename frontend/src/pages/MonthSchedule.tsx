import React, { useEffect, useState } from 'react'
import { Schedule, useApi } from '../service/api'
import {
  addMonths,
  formatDate,
  getDayOfWeek,
  getMonthDates, getMonthLabel,
  isWeekend,
  Month
} from '../dates'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { Alert, Button, ButtonGroup, Container, Tab, Table, Tabs } from 'react-bootstrap'
import { sortArrayBy } from '../sort-array-by'
import { Link } from 'react-router-dom'
import { faArrowLeft, faArrowRight, faEdit } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { workplaces } from '../domain'
import { Permission, usePermissions } from '../auth/auth'
import { Printer } from 'react-bootstrap-icons'

function getUserShiftEntries (schedule: Schedule): [string, number][] {
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
  return userShiftEntries
}

export function MonthSchedule (): JSX.Element {
  const api = useApi()
  const [month, setMonth] = useState<Month>({ year: 2023, month: 3 })
  const [schedule, setSchedule] = useState<Schedule | null | undefined>(undefined)
  const permissions = usePermissions()

  useEffect(() => {
    api.schedule.get(month)
      .then(setSchedule)
  }, [month])

  function handlePreviousMonth () {
    setSchedule(undefined)
    setMonth(prevMonth => addMonths(prevMonth, -1))
  }

  function handleNextMonth () {
    if (!permissions.includes(Permission.approveRequirementsOfOthers) && month.year === 2023 && month.month === 3) {
      return
    }

    setSchedule(undefined)
    setMonth(prevMonth => addMonths(prevMonth, 1))
  }

  return (<Container>
    <ButtonGroup className="no-print">
      <Button variant="outline-secondary" onClick={handlePreviousMonth}><FontAwesomeIcon icon={faArrowLeft}/></Button>
      <Button variant="outline-secondary" onClick={handleNextMonth}><FontAwesomeIcon icon={faArrowRight}/></Button>
    </ButtonGroup>

    {schedule === undefined
      ? <LoadingSpinner/>
      : <MonthScheduleInner month={month} schedule={schedule}/>}
  </Container>)
}

function MonthScheduleInner ({ month, schedule }: { month: Month, schedule: Schedule | null }): JSX.Element {
  const permissions = usePermissions()

  return <>
    <h1>{getMonthLabel(month)}</h1>

    {permissions.includes(Permission.modifySchedule) &&
      <div className="float-end no-print"><Link to={`/sluzby/${month.year}/${month.month}/editace`}><FontAwesomeIcon
        icon={faEdit}/>Upravit</Link></div>
    }

    {schedule === null && <Alert variant="warning">Pro tento měsíc zatím nejsou vygenerované služby</Alert>}

    {schedule !== null &&
      <Tabs
        defaultActiveKey="schedule"
        id="uncontrolled-tab-example"
        className="mb-3"
      >
        <Tab eventKey="schedule" title="Rozvrh" style={{ overflowY: 'scroll' }}>

          <div className="float-end">
            <button className="btn btn-primary no-print" onClick={window.print}>
              <Printer/>&nbsp;Tisk
            </button>
          </div>

          <Table bordered hover className="table-fit table-sm table-bordered">
            <thead className="thead-light table-primary table-bordered">
            <tr>
              <th></th>
              {workplaces.map(w => <th scope="col">{w}</th>)}
            </tr>
            </thead>
            <tbody>
            {getMonthDates(month).map(d =>
              (<tr className={isWeekend(d) ? 'table-success' : ''}>
                <th scope="row">{formatDate(d)} ({getDayOfWeek(d)})</th>
                {workplaces.map(w => <td>
                  {schedule?.shifts?.find(s => s.day === d && s.workplace === w)?.user.userName}
                </td>)}
              </tr>)
            )}
            </tbody>
          </Table>
        </Tab>
        <Tab eventKey="shiftCounts" title="Počty služeb">
          <Table bordered hover className="table-fit table-sm ">
            <tbody>
            {getUserShiftEntries(schedule).map(([userName, shifts]) =>
              (<tr>
                <th scope="row">{userName}</th>
                <td>{shifts}</td>
              </tr>)
            )}
            </tbody>
          </Table>
        </Tab>
      </Tabs>
    }
  </>
}
