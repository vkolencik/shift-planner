import React, { useEffect, useState } from 'react'
import { Leave, MonthRequirements, useApi, User } from '../service/api'
import { useUserList } from '../service/users'
import {
  addMonths,
  getDay,
  Day,
  getActiveMonth,
  getMonthLabel,
  getNumberOfDaysOfMonth,
  isWeekend,
  Month, getMonthDays, getMonthDates
} from '../dates'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { Alert, Button, ButtonGroup, Table } from 'react-bootstrap'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faArrowLeft, faArrowRight } from '@fortawesome/free-solid-svg-icons'

export const Leaves: React.FC = () => {
  const api = useApi()
  const users = useUserList()
  const [leaves, setLeaves] = useState<Leave[] | null>(null)
  const [month, setMonth] = useState<Month>(getActiveMonth)

  useEffect(() => {
    if (leaves === null) {
      api.leaves.getMonth(month).then(setLeaves)
    }
  }, [])

  if (leaves === null) {
    return <LoadingSpinner/>
  }

  const daysInMonth = getNumberOfDaysOfMonth(month)
  const days = getMonthDates(month)
  const weekends: Day[] = days.filter(day => isWeekend(day))

  const handlePreviousMonth = () => setMonth(prevMonth => addMonths(prevMonth, -1))
  const handleNextMonth = () => setMonth(prevMonth => addMonths(prevMonth, 1))

  async function handleAddLeave (userId: string, day: Day) {
    const leave = await api.leaves.add(userId, day)
    if (leaves === null) {
      throw new Error('Leaves is null')
    }
    setLeaves(prevLeaves => [...prevLeaves!, leave])
  }

  function DayCell (props: { userId: string, day: Day }) {
    const x = leaves!.find(l => l.day ===  props.day && l.userId === props.userId) !== undefined ? 'X' : ''
    return <td onClick={() => handleAddLeave(props.userId, props.day)}>
      {x}
    </td>
  }

  return <div className="container">
    <ButtonGroup>
      <Button variant="outline-secondary" onClick={handlePreviousMonth}><FontAwesomeIcon icon={faArrowLeft}/></Button>
      <Button variant="outline-secondary" onClick={handleNextMonth}><FontAwesomeIcon icon={faArrowRight}/></Button>
    </ButtonGroup>
    <h2>{getMonthLabel(month)}</h2>
    <Table bordered hover className='table-bordered'>
      <thead className="thead-light sticky-top table-primary table-bordered">
      <tr>
        <th>Datum</th>
        {days.map(day => weekends.includes(day)
          ? <th scope="col" className="weekend-day text-center">{day}</th>
          : <th scope="col" className="text-center">{day}</th>)}
      </tr>
      </thead>
      <tbody>
      {users.map(user =>
        <tr>
          <th scope="row">{user.userName}</th>
          {days.map(day => <DayCell userId={user.userId} day={day} />
          )}
        </tr>
      )}
      </tbody>
    </Table>
  </div>
}
