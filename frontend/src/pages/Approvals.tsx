import React, { useEffect, useState } from 'react'
import { MonthRequirements, useApi } from '../service/api'
import { Day, formatDate, getCurrentMonth, getCurrentMonthString, toDay } from '../dates'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { aggregateRequirements } from '../requirements/aggregate-requirements'
import { Alert, Badge, Button, Table } from 'react-bootstrap'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faUser } from '@fortawesome/free-solid-svg-icons'
import Modal from 'react-bootstrap/Modal'

type RequirementCoords = { userName: string, day: Day }

export function Approvals () {
  const [requirements, setRequirements] = useState<MonthRequirements | null>(null)
  const [hideApproved, setHideApproved] = useState<boolean>(false)
  const api = useApi()

  useEffect(() => {
    if (requirements === null) {
      api.requirements.getMonth(getCurrentMonth())
        .then(r => setRequirements(r))
    }
  }, [requirements])

  const [requirementToDelete, setRequirementToDelete] = useState<RequirementCoords | null>(null)

  if (requirements === null) {
    return <LoadingSpinner/>
  }

  const requirementsDisplayModel = aggregateRequirements(requirements, hideApproved)

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

    api.requirements.delete(userId, toDay(requirementToDelete.day))
      .then(() => {
        setRequirementToDelete(null)
        setRequirements(oldRequirements => oldRequirements?.filter(r => !(r.userId === userId && r.date === requirementToDelete.day)) || null)
      })
  }

  function handleApproval (userName: string, day: string) {
    const userId = requirements?.find(r => r.userName === userName)?.userId

    if (!userId) {
      throw new Error('Cannot find user to approve for')
    }

    api.requirements.approve(userId, toDay(day))
      .then(() => {
        setRequirements(oldRequirements => {
          if (oldRequirements === null) {
            return null
          }

          const newRequirements = [...oldRequirements]
          const approvedRequirement = newRequirements.find(r => r.userId === userId && r.date === day)
          if (approvedRequirement === undefined) {
            throw new Error('Cannot find the approved requirement')
          }
          approvedRequirement.approved = true
          return newRequirements
        })
      })
  }

  function handleChangeHideApproved (e: React.ChangeEvent<HTMLInputElement>) {
    setHideApproved(e.target.checked)
  }

  return <>
    <h1>Schvalování dovolených pro {getCurrentMonthString()}</h1>
    <div className="form-check form-switch mb-3">
      <input className="form-check-input" type="checkbox" id="switchHideApproved" onChange={handleChangeHideApproved}/>
      <label className="form-check-label" htmlFor="switchHideApproved">Skrýt schválené</label>
    </div>
    {userNames.length === 0 && <Alert variant="dark">Nejsou žádné dovolené ke schválení</Alert>}
    {userNames.map(userName => (<>
      <h2 key={'header-' + userName}><FontAwesomeIcon icon={faUser} style={{ fontSize: '80%' }}/>{userName}</h2>
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
            <td className='align-middle'>
              {leave.approved
                ? <Badge bg="success" pill>Schváleno</Badge>
                : <Badge bg="warning" text='dark' pill>Čeká na schválení</Badge>
              }

            </td>
            <td>
              {leave.approved || <>
                <Button variant="success" onClick={() => handleApproval(userName, leave.day)} className='me-1'>
                  Schválit
                </Button>
                <Button variant="danger" onClick={() => setRequirementToDelete({ userName: userName, day: leave.day })}>
                  Smazat
                </Button>
              </>}
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
          <Modal.Title>Zamítnutí (smazání) požadavku </Modal.Title>
        </Modal.Header>

        <Modal.Body>
          <p>Opravdu chcete zamítnout (smazat) požadavek uživatele <strong>{requirementToDelete.userName} v
            den {formatDate(requirementToDelete.day)}</strong>?</p>
          <p>Tato operace je nevratná.</p>
        </Modal.Body>

        <Modal.Footer>
          <Button variant="danger" className="me-auto" onClick={handleDeclineRequirement}>Smazat</Button>
          <Button variant="outline-secondary" onClick={() => {
            closeModal()
          }}>Zavřít</Button>
        </Modal.Footer>
      </Modal>
    }
  </>
}
