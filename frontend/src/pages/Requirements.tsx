import React, { useEffect, useRef, useState } from 'react'
import Button from 'react-bootstrap/Button'
import Modal from 'react-bootstrap/Modal'

import FullCalendar from '@fullcalendar/react' // must go before plugins
import { DateSelectArg, DateUnselectArg, EventClickArg } from '@fullcalendar/core'
import dayGridPlugin from '@fullcalendar/daygrid' // a plugin!
import interactionPlugin, { DateClickArg } from '@fullcalendar/interaction'
import bootstrap5Plugin from '@fullcalendar/bootstrap5'
import csLocale from '@fullcalendar/core/locales/cs'

import moment from 'moment'
import { EmployeeMonthRequirements, momentToDay, RequirementType, requirementTypes, useApi, User } from '../service/api'
import { EventSourceInput } from '@fullcalendar/core'
import { Alert, ButtonGroup, Card, Container, FormCheck, Spinner } from 'react-bootstrap'
import FormCheckLabel from 'react-bootstrap/FormCheckLabel'
import FormCheckInput from 'react-bootstrap/FormCheckInput'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { Permission, usePermissions } from '../auth/auth'
import {
  addMonths,
  getActiveMonth,
  getMonthLabel,
  getStartOfMonth, isMonthWritable,
  Month
} from '../dates'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faArrowLeft, faArrowRight } from '@fortawesome/free-solid-svg-icons'
import { UserSelectModal } from '../UserSelectModal'

type DayDate = moment.Moment

interface DateRange {
  from: DayDate
  to: DayDate
}

const DisplayDate: React.FC<{ date: DayDate }> = ({ date }: { date: DayDate }) => <>{date.format('D. M. YYYY')}</>

function requirementsToEvents (requirements: EmployeeMonthRequirements): EventSourceInput {
  const colorMapping: { [key in RequirementType]: string | undefined } = {
    'only-afternoon': '#0696c4',
    'only-morning': '#f1d567',
    'soft-yes': '#2eb401',
    'leave': '#c985de',
    'hard-no': '#9a0d0d'
  }

  return Object.entries(requirements).map(([date, type]) => ({
    id: date,
    title: requirementTypes[type].name,
    start: date,
    color: colorMapping[type]
  }))
}

export const Requirements: React.FC = () => {
  const calendarRef = useRef<any>(null)
  const [selectedDateRange, setSelectedDateRange] = useState<DateRange | null>(null)
  const [requirements, setRequirements] = useState<EmployeeMonthRequirements | null>(null)
  const [loading, setLoading] = useState<boolean>(false)
  const api = useApi()
  const [user, setUser] = useState<User>(api.users.loggedInUser)
  const permissions = usePermissions()
  const [userSelectOpen, setUserSelectOpen] = useState<boolean>(false)
  const [month, setMonth] = useState<Month>(getActiveMonth())

  const isSelectedMonthWritable = isMonthWritable(month)

  useEffect(() => {
    if (selectedDateRange === null && calendarRef.current) {
      calendarRef.current.getApi().unselect()
    }
  }, [selectedDateRange])

  useEffect(() => {
    setLoading(true)
    api.requirements.get(user.userId, month)
      .then(requirements => {
        setRequirements(requirements)
        setLoading(false)
      })
  }, [month.year, month.month, user.userId])

  const handleDateClick = (info: DateClickArg) => {
    handleDateRangeSelect({ from: moment(info.date), to: moment(info.date) })
  }

  function handleEventClick (event: EventClickArg) {
    handleDateRangeSelect({ from: moment(event.event.start), to: moment(event.event.end) })
  }

  function handleDateRangeSelect (dateRange: DateRange) {
    if (isSelectedMonthWritable || permissions.includes(Permission.modifyRequirementsOfOthers)) {
      setSelectedDateRange(dateRange)
    }
  }

  const handleDateRangeUnselect = (info: DateUnselectArg) => {
    setSelectedDateRange(null)
  }

  const handleAddRequirement = (type: RequirementType) => {
    if (selectedDateRange === null) {
      throw new Error('selectedDateRange is null')
    }

    setLoading(true)
    const day = momentToDay(selectedDateRange?.from)
    api.requirements.put(user.userId, day, type).then(() => {
      setLoading(false)
      setRequirements(oldRequirements => ({ ...oldRequirements, [day]: type }))
      setSelectedDateRange(null)
    })
  }

  const handleRemoveRequirement = () => {
    if (selectedDateRange === null) {
      throw new Error('selectedDateRange is null')
    }

    setLoading(true)
    const day = momentToDay(selectedDateRange?.from)
    api.requirements.delete(user.userId, day)
      .then(() => {
        setLoading(false)
        setRequirements(oldRequirements => {
          let newRequirements = { ...oldRequirements }
          delete newRequirements[day]
          return newRequirements
        })
        setSelectedDateRange(null)
      })
  }

  const handlePreviousMonth = () => {setMonth(oldMonth => addMonths(oldMonth, -1))}
  const handleCurrentMonth = () => {setMonth(getActiveMonth)}
  const handleNextMonth = () => {setMonth(oldMonth => addMonths(oldMonth, 1))}

  const oneDayRange = selectedDateRange !== null && selectedDateRange.from.isSame(selectedDateRange.to, 'day')

  const closeRequirementsModal = () => {
    setSelectedDateRange(null)
    setLoading(false)
  }

  const selectedDayRequirementType = selectedDateRange && requirements ? requirements[momentToDay(selectedDateRange?.from)] : null

  const handleUserSelected = (user: User) => {
    setUser(user)
    setUserSelectOpen(false)
  }

  return (
    <>
      {permissions.includes(Permission.modifyRequirementsOfOthers)
        && permissions.includes(Permission.readRequirementsOfOthers)
        && <>
          <Card title="Přepnout na jiného uživatele" style={{ margin: '10px', maxWidth: '24em' }}>
            <Card.Header>Zadávání za jiného uživatele</Card.Header>
            <Card.Body>
              <Card.Text>
                Zadáváte požadavky za uživatele <strong>{user?.userName}</strong>
              </Card.Text>
              <Button variant="secondary" onClick={() => setUserSelectOpen(true)}>Přepnout uživatele</Button>
            </Card.Body>
          </Card>
          <UserSelectModal isOpen={userSelectOpen} onHide={() => setUserSelectOpen(false)} onUserSelected={handleUserSelected}/>
        </>
      }

      {selectedDateRange &&
        <Modal
          show={true}
          onHide={() => {
            closeRequirementsModal()
          }}
          key="requirement-select-modal"
        >
          <Modal.Header closeButton>
            <Modal.Title>
              {oneDayRange
                ? <DisplayDate date={selectedDateRange.from}/>
                : <><DisplayDate date={selectedDateRange.from}/> - <DisplayDate date={selectedDateRange.to}/></>
              }
            </Modal.Title>
          </Modal.Header>

          <Modal.Body>
            {loading && <LoadingSpinner/>}

            {Object.values(requirementTypes).map(t => {
              return (
                <>
                  <FormCheck key={t.value}>
                    <FormCheckInput
                      type="radio"
                      onChange={() => handleAddRequirement(t.value)}
                      checked={selectedDayRequirementType === t.value}
                      id={`requirement-type-input-${t.value}`}
                      key={`radio-${t.value}`}
                    />
                    <FormCheckLabel
                      htmlFor={`requirement-type-input-${t.value}`}
                      key={`label-${t.value}`}>
                      {t.name}
                    </FormCheckLabel>
                  </FormCheck>
                </>
              )
            })}
          </Modal.Body>

          <Modal.Footer>
            {selectedDayRequirementType &&
              <Button variant="danger" className="me-auto" key="delete"
                      onClick={handleRemoveRequirement}>Odstranit</Button>}
            <Button variant="outline-secondary" key="close" onClick={() => {closeRequirementsModal() }}>Zavřít</Button>
          </Modal.Footer>
        </Modal>
      }

      <Container>
        <ButtonGroup>
          <Button variant="outline-secondary" onClick={handlePreviousMonth}>
            <FontAwesomeIcon icon={faArrowLeft}/>
          </Button>
          <Button variant="outline-secondary"
                  disabled={month.month === getActiveMonth().month && month.year === getActiveMonth().year}
                  onClick={handleCurrentMonth}>
            Aktuální měsíc
          </Button>
          <Button variant="outline-secondary" onClick={handleNextMonth}>
            <FontAwesomeIcon icon={faArrowRight}/>
          </Button>
        </ButtonGroup>

        <h2>{getMonthLabel(month)}</h2>

        {isSelectedMonthWritable ||
          <Alert variant="warning">Aktuální měsíc je již uzamčen (pouze pro čtení).</Alert>
        }

        {!loading && requirements
          ? <div><FullCalendar
            plugins={[dayGridPlugin, interactionPlugin, bootstrap5Plugin]}
            initialView="dayGridMonth"
            initialDate={getStartOfMonth(month)}
            themeSystem="bootstrap5"
            headerToolbar={false}
            firstDay={1}
            showNonCurrentDates={false}
            locale={csLocale}
            dateClick={handleDateClick}
            unselect={handleDateRangeUnselect}
            eventClick={handleEventClick}
            ref={calendarRef}
            height="auto"
            events={requirementsToEvents(requirements)}
          /></div>
          : <Spinner animation="border"/>
        }
      </Container>
    </>
  )
}
