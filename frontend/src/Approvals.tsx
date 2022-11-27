import React, { useEffect, useState } from 'react'
import { MonthRequirements, useApi } from './requirements/requirements-service'
import { currentMonth, currentYear, formatDate, getCurrentMonthString } from './dates'
import { LoadingSpinner } from './components/LoadingSpinner'
import { aggregateRequirements } from './requirements/aggregate-requirements'
import { Alert, Badge, Table } from 'react-bootstrap'
import { FaUser } from "react-icons/fa";

export function Approvals () {
  const [requirements, setRequirements] = useState<MonthRequirements | null>(null)
  const api = useApi()

  useEffect(() => {
    api.requirements.getMonth(currentYear, currentMonth)
      .then(r => setRequirements(r))
  }, [])

  if (requirements === null) {
    return <LoadingSpinner/>
  }

  const requirementsDisplayModel = aggregateRequirements(requirements)

  const userNames = Object.keys(requirementsDisplayModel)

  return <>
    <h1>Schvalování dovolených pro {getCurrentMonthString()}</h1>
    {userNames.length === 0 && <Alert variant="dark">Nejsou žádné dovolené ke schválení</Alert>}
    {userNames.map(userName => (<>
      <h2 key={'header-' + userName}><FaUser style={{fontSize: '80%'}} /> {userName}</h2>
      <Table striped bordered hover className='table-fit' key={'table' + userName}>
        <tbody>
        {requirementsDisplayModel[userName].map(leave =>
          <tr key={leave.day}>
            <td>{formatDate(leave.day)}</td>
            <td><Badge bg="secondary">Čeká na schválení</Badge></td>
          </tr>
        )}
        </tbody>
      </Table>
    </>))}
  </>
}
