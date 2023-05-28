import React, { useEffect, useState } from 'react'
import { MonthRequirements, RequirementType, useApi, User } from '../service/api'
import {
  getDay,
  getActiveMonth,
  getNumberOfDaysOfMonth,
  isWeekend,
  parseDate, Month, addMonths, getMonthLabel
} from '../dates'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { Button, ButtonGroup, ButtonToolbar, Table } from 'react-bootstrap'
import {Printer} from 'react-bootstrap-icons'
import './MonthlyOverview.css'
import { useUserList } from '../service/users'
import { isExternalUser } from '../users'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faArrowLeft, faArrowRight } from '@fortawesome/free-solid-svg-icons'
import { FixedPage, ScrollablePart } from '../layout/layout'

type CellDetails = { cssClass: string, titleText: string, cellText: string }
const getCellDetails = (requirementType: RequirementType): CellDetails => {
  switch (requirementType) {
    case 'leave':
      return ({
        cssClass: 'table-purple',
        cellText: 'D',
        titleText: 'Dovolená',
      })
    case 'soft-yes':
      return ({
        cssClass: 'table-success',
        cellText: 'Ch',
        titleText: 'Chci'
      })
    case 'hard-no':
      return ({
        cssClass: 'table-danger',
        cellText: 'N',
        titleText: 'Nemůžu'
      })
    case 'only-morning':
      return ({
        cssClass: 'table-warning',
        cellText: 'R',
        titleText: 'Pouze R'
      })
    case 'only-afternoon':
      return ({
        cssClass: 'table-primary',
        cellText: 'O',
        titleText: 'Jen odpoledne'
      })
  }
}

function DayCell ({ requirementType }: { requirementType: string | undefined }) {
  let details: CellDetails | null = null
  if (requirementType !== undefined) {
    // @ts-ignore
    details = getCellDetails(requirementType)
  }

  // @ts-ignore
  return <td className={`${details?.cssClass} text-center`} title={details?.titleText}>
    {details?.cellText}
  </td>
}
export function MonthlyOverview (): JSX.Element {
  const [month, setMonth] = useState<Month>(getActiveMonth())
  const [requirements, setRequirements] = useState<MonthRequirements | null>(null)
  const api = useApi()

  useEffect(() => {
    api.requirements.getMonth(month).then(setRequirements)
  }, [month])

  function handlePreviousMonth () {
    setRequirements(null)
    setMonth(prevMonth => addMonths(prevMonth, -1))
  }

  function handleNextMonth () {
    setRequirements(null)
    setMonth(prevMonth => addMonths(prevMonth, 1))
  }

  return (<FixedPage>
    <ButtonToolbar className='justify-content-between'>
      <ButtonGroup className="no-print" >
        <Button variant="outline-secondary" onClick={handlePreviousMonth}><FontAwesomeIcon icon={faArrowLeft}/></Button>
        <Button variant="outline-secondary" onClick={handleNextMonth}><FontAwesomeIcon icon={faArrowRight}/></Button>
      </ButtonGroup>
      <h1 className='h2'>{getMonthLabel(month)}</h1>

      <ButtonGroup className="no-print">
      <Button variant="primary" onClick={window.print}><Printer/>&nbsp;Tisk</Button>
    </ButtonGroup>
    </ButtonToolbar>

    {requirements === null
      ? <LoadingSpinner/>
      : <MonthlyOverviewInner month={month} requirements={requirements}/>}
  </FixedPage>)
}

export const MonthlyOverviewInner: React.FC<{requirements: MonthRequirements, month: Month}> = ({ month, requirements } ) => {
  const users = useUserList()

  const daysInMonth = getNumberOfDaysOfMonth(month)
  const days = new Array(daysInMonth).fill(1).map((_, i) => i + 1)

  const dailySums = new Array(daysInMonth).fill(users.filter(u => !isExternalUser(u)).length - 6)

  const requirementsPerUserDay: { [key: string]: string } = {}
  const countsAsAbsence = (r: RequirementType) => r === 'leave' || r === 'hard-no' || r === 'only-afternoon'
  for (const requirement of requirements) {
    const day = parseDate(requirement.date).getDate()
    if (countsAsAbsence(requirement.requirementType)) {
      dailySums[day - 1]--
    }
    requirementsPerUserDay[`${requirement.userId}-${day}`] = requirement.requirementType
  }

  const currentMonth = getActiveMonth()
  const weekends: number[] = days.filter(day => isWeekend(getDay(currentMonth.year, currentMonth.month, day)))

  return <ScrollablePart>
      <Table bordered hover className='table-responsive table-sm table-bordered' style={{width: '1px'}}>
        <thead className='thead-light table-primary table-bordered sticky-top'>
        <tr>
          <th>Datum</th>
          {days.map(day => weekends.includes(day)
            ? <th scope='col' className='weekend-day text-center'>{day}</th>
            : <th scope='col' className='text-center'>{day}</th>)}
        </tr>
        <tr>
          <th>Lidí k dispozici</th>
          {days.map(day => weekends.includes(day)
            ? <th scope="col" className="weekend-day">&nbsp;</th>
            : <th scope="col">{dailySums[day - 1]}</th>)}
        </tr>
        </thead>
        <tbody className=''>
        {users.map(user =>
          <tr>
            <th scope="row">{user.userName}</th>
            {days.map(day => <DayCell requirementType={requirementsPerUserDay[`${user.userId}-${day}`]}/>
            )}
          </tr>
        )}
        </tbody>
      </Table>
    </ScrollablePart>
}
