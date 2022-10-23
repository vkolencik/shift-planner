import React, { useEffect, useRef, useState } from 'react'
import FullCalendar, { EventClickArg } from '@fullcalendar/react' // must go before plugins
import dayGridPlugin from '@fullcalendar/daygrid' // a plugin!
import interactionPlugin, { DateClickArg } from '@fullcalendar/interaction'
import bootstrap5Plugin from '@fullcalendar/bootstrap5'
import csLocale from '@fullcalendar/core/locales/cs'
import { DateSelectArg, DateUnselectArg } from '@fullcalendar/common'
import Button from 'react-bootstrap/Button'
import Modal from 'react-bootstrap/Modal'
import moment from 'moment'
import {
  addRequirement,
  getRequirements,
  EmployeeMonthRequirements,
  RequirementType,
  requirementTypes, removeRequirement
} from './requirements/requirements-service'
import { EventSourceInput } from '@fullcalendar/core'
import { FormCheck, Spinner } from 'react-bootstrap'
import FormCheckLabel from 'react-bootstrap/FormCheckLabel'
import FormCheckInput from 'react-bootstrap/FormCheckInput'

type DayDate = moment.Moment

interface DateRange {
  from: DayDate
  to: DayDate
}

const DisplayDate: React.FC<{date: DayDate}> = ({ date }: { date: DayDate }) => <>{date.format('D. M. YYYY')}</>

function requirementsToEvents (requirements: EmployeeMonthRequirements): EventSourceInput {
  const colorMapping: {[key in RequirementType]: string | undefined } = {
    "only-afternoon": '#0696c4',
    "only-morning": '#9A7F0D',
    "soft-yes": '#2eb401',
    'leave': '#9A7F0D',
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
  const [requirements, setRequirements] = useState<EmployeeMonthRequirements>(getRequirements('bogus-user'))
  const [loading, setLoading] = useState<boolean>(false)

  useEffect(() => {
    if (selectedDateRange === null && calendarRef.current) {
      calendarRef.current.getApi().unselect()
    }
  }, [selectedDateRange])

  const handleDateRangeSelect = (info: DateSelectArg) => {
    setSelectedDateRange({ from: moment(info.start), to: moment(info.end).add(-1, 'day') })
  }

  const handleDateClick = (info: DateClickArg) => {
    setSelectedDateRange({ from: moment(info.date), to: moment(info.date) })
  }

  const handleDateRangeUnselect = (info: DateUnselectArg) => {
    setSelectedDateRange(null)
  }

  const handleAddRequirement = (type: RequirementType) => {
    if (selectedDateRange === null) {
      throw new Error('selectedDateRange is null')
    }

    addRequirement('bogus-user', selectedDateRange?.from.format('yyyy-MM-DD'), type)
    setLoading(true)

    setTimeout(() => {
      setLoading(false)
      setRequirements(getRequirements('bogus-user'))
      setSelectedDateRange(null)
    }, 500)
  }

  const handleRemoveRequirement = () => {
    if (selectedDateRange === null) {
      throw new Error('selectedDateRange is null')
    }

    removeRequirement('bogus-user', selectedDateRange?.from.format('yyyy-MM-DD'))
    setLoading(true)

    setTimeout(() => {
      setLoading(false)
      setRequirements(getRequirements('bogus-user'))
      setSelectedDateRange(null)
    }, 500)
  }

  const oneDayRange = selectedDateRange !== null && selectedDateRange.from.isSame(selectedDateRange.to, 'day')

  const closeModal = () => setSelectedDateRange(null)

  function handleEventClick (event: EventClickArg) {
    setSelectedDateRange({ from: moment(event.event.start), to: moment(event.event.end) })
  }

  const selectedDayRequirementType = selectedDateRange ? requirements[selectedDateRange?.from.format('yyyy-MM-DD')] : null

  return (
    <>
      <h1>Prosinec 2022</h1>

      {selectedDateRange &&
        <Modal
          show={true}
          onHide={() => {closeModal()}}>

          <Modal.Header closeButton>
            <Modal.Title>
              {oneDayRange
                ? <DisplayDate date={selectedDateRange.from}/>
                : <><DisplayDate date={selectedDateRange.from}/> - <DisplayDate date={selectedDateRange.to}/></>
              }
            </Modal.Title>
          </Modal.Header>

          <Modal.Body>
            {loading &&
              <div
                className="position-absolute top-0 start-0 w-100 h-100 d-flex flex-column align-items-center bg-white justify-content-center opacity-50"
                style={{ 'zIndex': 1000 }}>
                <Spinner animation="border"/>
              </div>
            }

            {Object.values(requirementTypes).map(t => {
              return (
                <>
                  <FormCheck key={t.value}>
                    <FormCheckInput
                      type="radio"
                      onClick={() => handleAddRequirement(t.value)}
                      checked={selectedDayRequirementType === t.value}
                      id={`requirement-type-input-${t.value}`}
                    />
                    <FormCheckLabel htmlFor={`requirement-type-input-${t.value}`}>{t.name}</FormCheckLabel>
                  </FormCheck>
                </>
              )
            })}
          </Modal.Body>

          <Modal.Footer>
            { selectedDayRequirementType && <Button variant="danger" className='me-auto' onClick={handleRemoveRequirement}>Odstranit</Button> }
            <Button variant="outline-secondary" onClick={() => {closeModal()}}>Zavřít</Button>
          </Modal.Footer>
        </Modal>
      }

      <div><FullCalendar
        plugins={[dayGridPlugin, interactionPlugin, bootstrap5Plugin]}
        initialView="dayGridMonth"
        initialDate={new Date(2022, 11, 1)}
        themeSystem="bootstrap5"
        headerToolbar={false}
        firstDay={1}
        showNonCurrentDates={false}
        locale={csLocale}
        dateClick={handleDateClick}

        // selectable={true}
        // select={handleDateRangeSelect}
        unselect={handleDateRangeUnselect}
        eventClick={handleEventClick}
        ref={calendarRef}
        height='auto'
        events={requirementsToEvents(requirements)}
      /></div>
    </>
  )
}
