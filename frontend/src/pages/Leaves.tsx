import React, { useEffect, useState } from 'react'
import { Leave, useApi } from '../service/api'
import { useUserList } from '../service/users'
import {
  addMonths, compareMonths,
  Day,
  formatDate,
  getActiveMonth, getDayOfWeekNumber, getMonth,
  getMonthDates,
  getMonthDay,
  getMonthLabel, isMonthWritable,
  isWeekend,
  Month
} from '../dates'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { Alert, Button, ButtonGroup, Table } from 'react-bootstrap'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faArrowLeft, faArrowRight } from '@fortawesome/free-solid-svg-icons'
import './Leaves.css'
import Modal from 'react-bootstrap/Modal'
import { toast } from 'react-toastify'

const minEmployeesPerWeekday = [
  36,
  36,
  37,
  34,
  32,
  32,
  32
]

const minEmployeesInSummer = 32

const getMinAvailableEmployeesForDay = (day: Day): number => {
  const month = getMonth(day)

  const monthDay = getMonthDay(day)
  if (compareMonths(month, {year: 2023, month: 8}) == 0 && 14 <= monthDay && monthDay <= 31) {
    return minEmployeesInSummer + 3
  }

  if (compareMonths(month, {year: 2023, month: 7}) == 0 && 17 <= monthDay && monthDay <= 21) {
    return minEmployeesInSummer + 2
  }

  if (month.month === 7 || month.month === 8) {
    return minEmployeesInSummer
  }

  return minEmployeesPerWeekday[getDayOfWeekNumber(day) - 1]
}

let minWritableMonth: Month = getActiveMonth()
if (!isMonthWritable(minWritableMonth)) {
  minWritableMonth = addMonths(minWritableMonth, 1)
}

const maxMonth: Month = { year: minWritableMonth.year, month: 12 }

export const Leaves: React.FC = () => {

  const api = useApi()
  const allUsers = useUserList()
  const [leaves, setLeaves] = useState<Leave[] | null>(null)
  const [month, setMonth] = useState<Month>(minWritableMonth)
  const [loading, setLoading] = useState<boolean>(false)
  const [leaveToDelete, setLeaveToDelete] = useState<Day | null>(null)

  useEffect(() => {
    setLoading(true)
    api.leaves.getMonth(month).then(leaves => {
      setLeaves(leaves)
      setLoading(false)
    })
  }, [month])

  if (leaves === null) {
    return <LoadingSpinner/>
  }

  const users = allUsers.filter(u => compareMonths(u.activeUntil, month) >= 0)

  const isReadOnly = compareMonths(month, minWritableMonth) < 0

  const days = getMonthDates(month)
  const weekends: Day[] = days.filter(day => isWeekend(day))

  const leavesPerDay = Object.fromEntries(days.map(d => [d, 0]))
  for (const leave of leaves) {
    leavesPerDay[leave.day]++
  }

  const handlePreviousMonth = () => setMonth(prevMonth => addMonths(prevMonth, -1))

  const handleNextMonth = () => setMonth(prevMonth => compareMonths(prevMonth, maxMonth) === 0 ? prevMonth : addMonths(prevMonth, 1))

  const handleOwnCellClick = async (day: Day) => {
    const hasLeave = leaves!.find(l => l.day === day && l.userId === api.users.loggedInUser.userId) !== undefined

    if (hasLeave) {
      handleRemoveLeave(day)
    } else {
      handleAddLeave(day)
    }
  }

  const handleAddLeave = async (day: Day) => {
    const minAvailableEmployees = getMinAvailableEmployeesForDay(day)
    const leavesInDay = leavesPerDay[day]
    if (users.length - leavesInDay - 1 < minAvailableEmployees) {
      toast.error(`Překročen maximální počet dovolených v daný den (celkem ${users.length} zaměstnanců, ${leavesInDay} již naplánovaných dovolených, je třeba minimálně ${minAvailableEmployees} zaměstnanců k dipozici)`)
      return
    }

    setLoading(true)
    const leave = await api.leaves.add(api.users.loggedInUser.userId, day)
    if (leaves === null) {
      throw new Error('Leaves is null')
    }
    setLoading(false)
    setLeaves(prevLeaves => [...prevLeaves!, leave])
  }

  const handleRemoveLeave = (day: Day) => { setLeaveToDelete(day) }

  const handleDeleteLeaveConfirmation = async () => {
    if (leaveToDelete === null) {
      throw new Error('No leave to delete')
    }
    setLoading(true)
    const userId = api.users.loggedInUser.userId
    await api.leaves.delete(userId, leaveToDelete)
    setLeaves(prevLeaves => prevLeaves?.filter(l => !(l.day === leaveToDelete && l.userId === userId)) || null)
    setLeaveToDelete(null)
    setLoading(false)
  }

  function DayCell (props: { userId: string, day: Day }) {
    const usersOwnRow = props.userId === api.users.loggedInUser.userId
    const hasLeave = leaves!.find(l => l.day === props.day && l.userId === props.userId) !== undefined
    if (!usersOwnRow || isReadOnly) {
      return <td className={hasLeave ? 'leave' : ''}></td>
    } else {
      return <td
        className={hasLeave ? 'leave' : ''}
        onClick={() => handleOwnCellClick(props.day)}>
      </td>
    }
  }

  const closeModal = () => { setLeaveToDelete(null) }

  return <div className="container">
    {loading && <LoadingSpinner/>}

    {leaveToDelete &&
      <Modal
        show={true}
        onHide={() => {
          closeModal()
        }}>

        <Modal.Header closeButton>
          <Modal.Title>Smazání dovolené</Modal.Title>
        </Modal.Header>

        <Modal.Body>
          <p>Opravdu chcete smazat dovolenou <strong>{formatDate(leaveToDelete)}</strong>?</p>
        </Modal.Body>

        <Modal.Footer>
          <Button variant="danger" className="me-auto" onClick={handleDeleteLeaveConfirmation}>Smazat</Button>
          <Button variant="outline-secondary" onClick={() => {
            closeModal()
          }}>Zavřít</Button>
        </Modal.Footer>
      </Modal>
    }

    {isReadOnly && <Alert variant="warning">Pro tento měsíc již nelze zadávat dovolené.</Alert>}

    <ButtonGroup>
      <Button variant="outline-secondary" onClick={handlePreviousMonth}><FontAwesomeIcon icon={faArrowLeft}/></Button>
      <Button variant="outline-secondary" onClick={handleNextMonth}
              disabled={compareMonths(month, maxMonth) === 0}><FontAwesomeIcon icon={faArrowRight}/></Button>
    </ButtonGroup>
    <h2>{getMonthLabel(month)}</h2>
    <Table bordered hover className="table-bordered">
      <thead className="thead-light sticky-top table-primary table-bordered">
      <tr>
        <th>Datum</th>
        {days.map(day => weekends.includes(day)
          ? <th scope="col" className="weekend-day text-center">{getMonthDay(day)}.</th>
          : <th scope="col" className="text-center">{getMonthDay(day)}.</th>)}
      </tr>
      <tr className="subtitle">
        <th>dovolené&nbsp;/&nbsp;maximum</th>
        {days.map(day => {
          return <th scope="col" className={`${weekends.includes(day) ? 'weekend-day' : ''} text-center`}>
            {leavesPerDay[day]}/{users.length - getMinAvailableEmployeesForDay(day)}
          </th>

        })}
      </tr>
      </thead>
      <tbody>
      {users.map(user =>
        <tr className={user.userId === api.users.loggedInUser.userId ? 'own-row' : ''}>
          <th scope="row">{user.userName}</th>
          {days.map(day => <DayCell userId={user.userId} day={day}/>)}
        </tr>
      )}
      </tbody>
    </Table>
  </div>
}
