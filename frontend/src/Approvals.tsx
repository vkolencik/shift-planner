import { useEffect, useState } from 'react'
import { MonthRequirements, useApi } from './requirements/requirements-service'
import { currentMonth, currentYear } from './CurrentMonth'
import { LoadingSpinner } from './components/LoadingSpinner'
import React from 'react'

export function Approvals () {
  const [requirements, setRequirements] = useState<MonthRequirements | null>(null)
  const api = useApi()

  useEffect(() => {
    api.requirements.getMonth(currentYear, currentMonth + 1)
      .then(r => setRequirements(r))
  }, [])

  if (requirements === null) {
    return <LoadingSpinner/>
  }

  return <pre>
    {requirements.map(r => JSON.stringify(r) + '\n')}
  </pre>
}
