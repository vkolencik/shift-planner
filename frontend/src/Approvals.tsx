import React, { useEffect, useState } from 'react'
import { MonthRequirements, requirementTypes, useApi } from './requirements/requirements-service'
import { currentMonth, currentYear, formatDate, getCurrentMonthString, toDay } from './dates'
import { LoadingSpinner } from './components/LoadingSpinner'
import { aggregateRequirements } from './requirements/aggregate-requirements'
import { Alert, Badge, Button, FormCheck, Table } from 'react-bootstrap'
import { FaUser } from 'react-icons/fa'
import Modal from 'react-bootstrap/Modal'

type RequirementCoords = { userName: string, day: string }

export function Approvals () {
  const [requirements, setRequirements] = useState<MonthRequirements | null>(null)
  const api = useApi()

  useEffect(() => {
    if (requirements === null) {
      api.requirements.getMonth(currentYear, currentMonth)
        .then(r => setRequirements(r))
    }
  }, [requirements])

  const [requirementToDelete, setRequirementToDelete] = useState<RequirementCoords | null>(null)

  if (requirements === null) {
    return <LoadingSpinner/>
  }

  const requirementsDisplayModel = aggregateRequirements(requirements)

  const userNames = Object.keys(requirementsDisplayModel)

  const closeModal = () => {
    setRequirementToDelete(null)
  }

  const handleDeclineRequirement = () => {
    if (requirementToDelete === null) {
      throw new Error('No requirement specified for deletion')
    }

    const userId = requirements.find(r => r.userName === requirementToDelete.userName)?.userId

    if (!userId) {
      throw new Error('Cannot find user to delete for')
    }

    api.requirements.deleteForUser(userId, toDay(requirementToDelete.day))
      .then(() => {
        setRequirementToDelete(null)
        setRequirements(null)
      })
  }

  return <>
    <h1>Schvalování dovolených pro {getCurrentMonthString()}</h1>
    {userNames.length === 0 && <Alert variant="dark">Nejsou žádné dovolené ke schválení</Alert>}
    {userNames.map(userName => (<>
      <h2 key={'header-' + userName}><FaUser style={{ fontSize: '80%' }}/> {userName}</h2>
      <Table striped bordered hover className="table-fit" key={'table' + userName}>
        <thead>
        <tr>
          <th>Datum</th>
          <th>Stav</th>
          <th></th>
        </tr>
        </thead>
        <tbody>
        {requirementsDisplayModel[userName].map(leave =>
          <tr key={leave.day}>
            <td>{formatDate(leave.day)}</td>
            <td><Badge bg="secondary">Čeká na schválení</Badge></td>
            <td>
              <Button variant="danger" onClick={() => setRequirementToDelete({ userName: userName, day: leave.day })}>
                Smazat
              </Button>
            </td>
          </tr>
        )}
        </tbody>
      </Table>
    </>))}

    {requirementToDelete &&
      <Modal
        show={true}
        onHide={() => {
          closeModal()
        }}>

        <Modal.Header closeButton>
          <Modal.Title>Zamítnutí (odstranění) požadavku </Modal.Title>
        </Modal.Header>

        <Modal.Body>
          <p>Opravdu chcete zamítnout (smazat) požadavek uživatele <strong>{requirementToDelete.userName} v
            den {formatDate(requirementToDelete.day)}</strong>?</p>
          <p>Tato operace je nevratná.</p>
        </Modal.Body>

        <Modal.Footer>
          <Button variant="danger" className="me-auto" onClick={handleDeclineRequirement}>Zamítnout (Odstranit)</Button>
          <Button variant="outline-secondary" onClick={() => {
            closeModal()
          }}>Zavřít</Button>
        </Modal.Footer>
      </Modal>
    }
  </>
}
