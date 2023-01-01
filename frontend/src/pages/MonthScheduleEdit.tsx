import React, { useEffect, useState } from 'react'
import { Schedule, useApi, User, ValidationError } from '../service/api'
import {
  Day,
  formatDate,
  getDayOfWeek,
  getMonthDates, getMonthLabel,
  isWeekend,
  Month, toDay
} from '../dates'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { Alert, Button, Card, Form, Tab, Table, Tabs } from 'react-bootstrap'
import { sortArrayBy } from '../sort-array-by'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { faArrowLeft } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { workplaces } from '../domain'
import { UserSelectModal } from '../UserSelectModal'
import { ListUl } from 'react-bootstrap-icons'

interface CellCoords {
  day: Day,
  workplace: string
}

export function MonthScheduleEdit (): JSX.Element {
  const params = useParams()
  const [schedule, setSchedule] = useState<Schedule | null | undefined>(undefined)
  const api = useApi()
  const [highlightedUserId, setHighlightedUserId] = useState<string | null>(null)
  const [editedCoords, setEditedCoords] = useState<CellCoords | null>(null)
  const [validationErrors, setValidationErrors] = useState<ValidationError[] | null>(null)

  if (params.year === undefined || params.month === undefined) {
    throw new Error('Month params not present')
  }

  const month: Month = { year: parseInt(params.year), month: parseInt(params.month) }

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

  function ShiftCell ({ user, coords }: { user: User, coords: CellCoords }) {
    return <td
      key={`${coords.workplace}-${coords.day}`}
      className={user.userId === highlightedUserId ? 'bg-warning text-white' : ''}
      style={{ cursor: 'pointer' }}
      onMouseOver={() => setHighlightedUserId(user.userId)}
      onMouseOut={() => setHighlightedUserId(null)}
      onClick={() => setEditedCoords(coords)}
    >
      {user.userName}
    </td>
  }

  function handleCellUserSelected (user: User) {
    setSchedule(oldSchedule =>
      ({
        shifts: oldSchedule!.shifts.map(s => {
          if (s.day === editedCoords?.day && s.workplace === editedCoords.workplace) {
            return { day: s.day, workplace: s.workplace, user: user }
          } else {
            return { day: s.day, workplace: s.workplace, user: s.user }
          }
        })
      }))
    setEditedCoords(null)
  }

  function handleValidation () {
    if (schedule === null || schedule === undefined) {
      throw new Error('Schedule not present for validation')
    }

    api.schedule.validate(schedule)
      .then(setValidationErrors)
  }

  return <>
    <UserSelectModal isOpen={editedCoords !== null} onHide={() => setEditedCoords(null)}
                     onUserSelected={handleCellUserSelected}/>

    <h1>{getMonthLabel(month)}</h1>

    <div className="float-end"><Link to={`/sluzby`}><FontAwesomeIcon icon={faArrowLeft}/>Zpět na služby</Link></div>

    <Tabs
      defaultActiveKey="schedule"
      id="uncontrolled-tab-example"
      className="mb-3"
    >
      <Tab eventKey="schedule" title="Rozvrh" style={{ overflowY: 'scroll' }}>

        <Table bordered className="table-fit table-sm table-bordered">
          <thead className="thead-light table-primary table-bordered">
          <tr>
            <th></th>
            {workplaces.map(w => <th scope="col">{w}</th>)}
          </tr>
          </thead>
          <tbody>
          {getMonthDates(month).map((d) =>
            (<tr className={isWeekend(d) ? 'table-success' : ''}>
              <th scope="row">{formatDate(d)} ({getDayOfWeek(d)})</th>
              {workplaces.map(w =>
                <ShiftCell
                  user={schedule?.shifts?.find(s => s.day === d && s.workplace === w)?.user!}
                  coords={{ day: d, workplace: w }}
                />
              )}
            </tr>)
          )}
          </tbody>
        </Table>

        <Card style={{ maxWidth: '36em' }}>
          <Card.Header>Uložení rozvrhu</Card.Header>
          <Card.Body>
            <Button
              variant="secondary"
              onClick={handleValidation}
            >
              Zvalidovat
            </Button>
            {validationErrors && <ul>
              {validationErrors.map(e => <li>{e.shifts.map(s => formatDate(s.day)).join(',')}{e.user === null ? '' : `(${e.user.userName})`}</li>)}
            </ul>}
          </Card.Body>
        </Card>

      </Tab>
      <Tab eventKey="shiftCounts" title="Počty služeb">
        <Table bordered className="table-fit table-sm ">
          <tbody>
          {userShiftEntries.map(([userName, shifts]) =>
            (<tr>
              <th scope="row">{userName}</th>
              <td>{shifts}</td>
            </tr>)
          )}
          </tbody>
        </Table>
      </Tab>
    </Tabs>
  </>
}
