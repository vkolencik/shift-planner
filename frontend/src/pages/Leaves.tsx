import React, { useEffect, useState } from 'react'
import { Leave, useApi } from '../service/api'
import { useUserList } from '../service/users'
import {
  addMonths,
  Day,
  formatDate,
  getActiveMonth,
  getMonthDates,
  getMonthDay,
  getMonthLabel,
  isWeekend,
  Month
} from '../dates'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { Button, ButtonGroup, Table } from 'react-bootstrap'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faArrowLeft, faArrowRight } from '@fortawesome/free-solid-svg-icons'
import './Leaves.css'
import Modal from 'react-bootstrap/Modal'
import { toast } from 'react-toastify'

const maxLeavesPerDay = 1

export const Leaves: React.FC = () => {
  const api = useApi()
  const users = useUserList()
  const [leaves, setLeaves] = useState<Leave[] | null>(null)
  const [month, setMonth] = useState<Month>(getActiveMonth)
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

  const days = getMonthDates(month)
  const weekends: Day[] = days.filter(day => isWeekend(day))

  const leavesPerDay = Object.fromEntries(days.map(d => [d, 0]))
  for (const leave of leaves) {
    leavesPerDay[leave.day]++
  }

  const handlePreviousMonth = () => setMonth(prevMonth => addMonths(prevMonth, -1))
  const handleNextMonth = () => setMonth(prevMonth => addMonths(prevMonth, 1))

  const handleOwnCellClick = async (day: Day) => {
    const hasLeave = leaves!.find(l => l.day === day && l.userId === api.users.loggedInUser.userId) !== undefined

    if (hasLeave) {
      handleRemoveLeave(day)
    } else {
      handleAddLeave(day)
    }
  }

  const handleAddLeave = async (day: Day) => {
    if (leavesPerDay[day] >= maxLeavesPerDay) {
      toast.error(`Překročen maximální počet dovolených v daný den (${maxLeavesPerDay})`)
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
    if (!usersOwnRow) {
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

    <ButtonGroup>
      <Button variant="outline-secondary" onClick={handlePreviousMonth}><FontAwesomeIcon icon={faArrowLeft}/></Button>
      <Button variant="outline-secondary" onClick={handleNextMonth}><FontAwesomeIcon icon={faArrowRight}/></Button>
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
        <th>Počet dovolených</th>
        {days.map(day => weekends.includes(day)
          ? <th scope="col" className="weekend-day text-center">{leavesPerDay[day]}</th>
          : <th scope="col" className="text-center">{leavesPerDay[day]}</th>)}
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
