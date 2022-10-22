import React, { useEffect, useRef, useState } from 'react'
import FullCalendar from '@fullcalendar/react' // must go before plugins
import dayGridPlugin from '@fullcalendar/daygrid' // a plugin!
import interactionPlugin, { DateClickArg } from '@fullcalendar/interaction'
import bootstrap5Plugin from '@fullcalendar/bootstrap5'
import csLocale from '@fullcalendar/core/locales/cs'
import { DateSelectArg, DateUnselectArg } from '@fullcalendar/common'
import Button from 'react-bootstrap/Button'
import Modal from 'react-bootstrap/Modal'
import moment from 'moment'

type DayDate = moment.Moment

interface DateRange {
  from: DayDate
  to: DayDate
}

const DisplayDate: React.FC<{date: DayDate}> = ({ date }: { date: DayDate }) => <>{date.format('D. M. YYYY')}</>

export const Requirements: React.FC = () => {
  const calendarRef = useRef<any>(null)
  const [selectedDateRange, setSelectedDateRange] = useState<DateRange | null>(null)

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

  const oneDayRange = selectedDateRange !== null && selectedDateRange.from.isSame(selectedDateRange.to, 'day')

  const closeModal = () => setSelectedDateRange(null)

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
            <ul>
              <li><a href="#" onClick={() => {setSelectedDateRange(null)}}>Dovolená</a></li>
              {oneDayRange && <>
                <li><a href="#" onClick={() => {setSelectedDateRange(null)}}>Chci</a></li>
                <li><a href="#" onClick={() => {setSelectedDateRange(null)}}>Nemůžu</a></li>
                <li><a href="#" onClick={() => {setSelectedDateRange(null)}}>Pouze R</a></li>
                <li><a href="#" onClick={() => {setSelectedDateRange(null)}}>Až odpoledne</a></li>
              </>
              }
            </ul>
          </Modal.Body>

          <Modal.Footer>
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
        ref={calendarRef}
        height='auto'
        events={[
          {
            id: '1',
            title: 'chci',
            start: '2022-12-06'
          },
          {
            id: '2',
            title: 'chci',
            start: '2022-12-08'
          },
          {
            id: '3',
            title: 'dovolená',
            start: '2022-12-21',
            end: '2022-12-23',
            color: '#9A7F0D'
          },
          {
            id: '4',
            title: 'nemůžu',
            start: '2022-12-13',
            color: '#9a0d0d'
          }
        ]}
      /></div>
    </>
  )
}
