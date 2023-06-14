import React, { useEffect, useState } from 'react'
import { Schedule, useApi, User, ValidationError } from '../service/api'
import {
  Day,
  formatDate,
  getDayOfWeekName,
  getMonthDates, getMonthLabel,
  isWeekend,
  Month, toDay
} from '../dates'
import { LoadingSpinner } from '../components/LoadingSpinner'
import {
  Alert,
  Button,
  Card,
  Col,
  Container,
  Form,
  ListGroup,
  ListGroupItem,
  Row, Spinner,
  Tab,
  Table,
  Tabs
} from 'react-bootstrap'
import { sortArrayBy } from '../sort-array-by'
import { Link, useParams } from 'react-router-dom'
import { faArrowLeft } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { workplaces } from '../domain'
import { UserSelectModal } from '../UserSelectModal'
import { toast } from 'react-toastify'

interface CellCoords {
  day: Day,
  workplace: string
}

function getErrorMessage (error: string): string {
  const messageMapping: { [key: string]: string } = {
    Unknown: 'Jiná chyba',
    EveryDayIsCovered: 'Nepokryty všechny směny',
    NoConcurrentShifts: 'Více paralelních směn uživatele',
    NoConsecutiveShifts: 'Po sobě jdoucí směny',
    NoAlternativeDayShifts: 'Obkročka mimo víkend',
    AlwaysAlternatesOverWeekends: 'Chybějící obkročka o víkendu',
    WeekendLimit: 'Překročen limit víkendových služeb',
    MinNumberOfWeekends: 'Nedostatečný počet víkendových služeb',
    NoSubsequentWeekends: 'Dva po sobě jdoucí víkendy',
    Leaves: 'Dovolená',
    WorkplaceCapability: 'Nedodržena kvalifikace na pracovišti',
    CompetenceGroupQualification: 'Chybí alespoň 1 uživatel s kvalifikací 1',
    PredefinedShifts: 'Neodpovídá předdefinovaným službám',
    MonthlyShiftCount: 'Nesprávný celkový počet směn v měsíci',
    WorkplacePreferencePercentage: 'Nedodržena procentuální preference na pracovišti',
    EvenSpreadWithinMonth: 'Nedodrženo rovnoměrné rozložení služeb v měsíci'
  }

  return messageMapping[error] || 'Neznámá chyba'
}

export function MonthScheduleEdit (): JSX.Element {
  const params = useParams()
  const [schedule, setSchedule] = useState<Schedule | null | undefined>(undefined)
  const api = useApi()
  const [highlightedUserId, setHighlightedUserId] = useState<string | null>(null)
  const [editedCoords, setEditedCoords] = useState<CellCoords | null>(null)
  const [isValidating, setIsValidating] = useState<boolean>(false)
  const [isSaving, setIsSaving] = useState<boolean>(false)
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
    const isUserHighlighted = user.userId === highlightedUserId
    const isError = validationErrors !== null
      && validationErrors.find(e => e.shifts.find(s => s.day === coords.day && s.workplace === coords.workplace) !== undefined) !== undefined

    const classNames = []
    if (isUserHighlighted) {
      classNames.push('bg-warning', 'text-white')
    }

    if (isError) {
      classNames.push('border', 'border-danger', 'border-3')
    }

    function handleCellClick () {
      if (!isSaving) {
        setEditedCoords(coords)
      }
    }

    return <td
      key={`${coords.workplace}-${coords.day}`}
      className={classNames.join(' ')}
      style={{ cursor: 'pointer' }}
      onMouseOver={() => setHighlightedUserId(user.userId)}
      onMouseOut={() => setHighlightedUserId(null)}
      onClick={handleCellClick}
    >
      {user.userName}
    </td>
  }

  function handleCellUserSelected (user: User) {
    if (isValid) {
      // Was valid but got dirty
      setValidationErrors(null)
    }

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

    setIsValidating(true)
    api.schedule.validate(month, schedule)
      .then(errors => {
        setValidationErrors(errors)
      })
      .finally(() => {setIsValidating(false)})
  }

  function handleSave () {
    if (schedule === null || schedule === undefined) {
      throw new Error('Schedule not present for saving')
    }

    setIsSaving(true)
    api.schedule.save(month, schedule)
      .then(() => {
        toast.success('Rozvrh byl uložen')
      })
      .finally(() => {setIsSaving(false)})
  }

  const isValid = validationErrors !== null && validationErrors.length === 0
  const isPassable = validationErrors !== null

  return <>
    <UserSelectModal
      isOpen={editedCoords !== null}
      onHide={() => setEditedCoords(null)}
      onUserSelected={handleCellUserSelected}/>

    <h1>{getMonthLabel(month)}</h1>

    <div className="float-end"><Link to={`/sluzby`}><FontAwesomeIcon icon={faArrowLeft}/>Zpět na služby</Link></div>

    <Tabs
      defaultActiveKey="schedule"
      id="uncontrolled-tab-example"
      className="mb-3"
    >
      <Tab eventKey="schedule" title="Rozvrh" style={{ overflowY: 'scroll' }}>
        <Container>
          <Row>

            <Col className="col-md">
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
                    <th scope="row">{formatDate(d)} ({getDayOfWeekName(d)})</th>
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
            </Col>

            <Col className="col-md">
              <Card
                style={{ maxWidth: '36em' }}
                border={validationErrors === null ? 'secondary' : (validationErrors.length === 0 ? 'success' : 'danger')}>
                <Card.Header>Uložení rozvrhu</Card.Header>

                {!isValidating && <>
                  {validationErrors !== null && validationErrors.length > 0 && (
                    <>
                      <Card.Body className="text-white bg-danger">
                        <Card.Title>Chyby v rozvrhu ({validationErrors.length})</Card.Title>
                      </Card.Body>
                      <ListGroup as="ul" variant="flush">
                        {validationErrors.map(e =>
                          <ListGroupItem as="li" className="text-danger">
                            <strong>
                              {Array.from(new Set(e.shifts.map(s => s.day))).map(d => formatDate(d)).join(', ')}
                              {e.user === null ? '' : ` (${e.user.userName})`}:
                            </strong>
                            {' '}{getErrorMessage(e.error)}
                          </ListGroupItem>)
                        }
                      </ListGroup>
                    </>)
                  }

                  {isValid &&
                    <Card.Body className="bg-success text-white">Rozvrh je bez chyb</Card.Body>
                  }
                </>}
                <Card.Body>
                  <Button
                    variant="light"
                    onClick={handleValidation}
                    disabled={isValidating}
                  >
                    {isValidating &&
                      <><span className="spinner-border spinner-border-sm" role="status"
                              aria-hidden="true"></span>&nbsp;</>}
                    Zvalidovat
                  </Button>

                  {isPassable &&
                    <Button
                      variant="success"
                      className="ms-3"
                      onClick={handleSave}
                      disabled={isSaving}
                    >
                      {isSaving &&
                        <><span className="spinner-border spinner-border-sm" role="status"
                                aria-hidden="true"></span>&nbsp;</>}
                      Uložit
                    </Button>
                  }

                </Card.Body>
              </Card>
            </Col>
          </Row>
        </Container>
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
