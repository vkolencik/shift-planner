import { Button, Table } from 'react-bootstrap'
import React, { ReactNode } from 'react'
import { Day, formatDate, toDay } from './dates'
import { faPlusCircle } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import Linkify from 'react-linkify'

interface ConferenceInfo {
  name: string
  dateFrom: Day | null
  dateTo: Day | null
  location: string | null
  info: string
  participants: string[]
}

const newTabLinkDecorator = (decoratedHref: string, decoratedText: string, key: number): ReactNode => {
  return (
    <a href={decoratedHref} key={key} target="_blank">
      {decoratedText}
    </a>
  )
}

const ConferencesTable = (props: { conferences: ConferenceInfo[] }) => {
  return <Table bordered hover className="table-responsive table-sm table-bordered">
    <thead>
    <tr>
      <th>Název</th>
      <th>Termín</th>
      <th>Místo</th>
      <th>Info</th>
      <th>&nbsp;</th>
      <th>Zájemci</th>
    </tr>
    </thead>
    <tbody>
    {props.conferences.map(ci => (<tr>
      <td>{ci.name}</td>
      <td>{[ci.dateFrom, ci.dateTo].map(ci => ci ? formatDate(ci) : null).filter(ci => ci).join(' - ') || 'Bude upřesněno'}</td>
      <td>{ci.location || 'Bude upřesněno'}</td>
      <td><Linkify componentDecorator={newTabLinkDecorator}>{ci.info}</Linkify></td>
      <td className="text-center"><Button><FontAwesomeIcon icon={faPlusCircle}/>&nbsp;Chci</Button></td>
      <td>{ci.participants.join(', ')}</td>
    </tr>))}
    </tbody>
  </Table>

}

export const Conferences = () => {
  return (<>
    <h1>Konference</h1>

    <h2>Konference ČR - A</h2>
    <ConferencesTable conferences={[
      {
        name: 'ČSARIM',
        dateFrom: toDay('2023-10-05'),
        dateTo: toDay('2023-10-07'),
        info: 'https://www.csarim.cz/akce',
        location: 'Praha',
        participants: ['John Doe', 'Jane Doe']
      },
      {
        name: 'Colours of Sepsis 2024',
        dateFrom: toDay('2024-01-22'),
        dateTo: toDay('2024-01-26'),
        info: 'https://coloursofsepsis.cz/',
        location: 'Ostrava',
        participants: ['John Doe', 'Jane Doe']
      },
      {
        name: 'ČSIM',
        dateFrom: toDay('2024-06-16'),
        dateTo: toDay('2024-06-18'),
        info: 'www.csim.cz/kongres/',
        location: 'Mikulov',
        participants: ['John Doe', 'Jane Doe']
      },
      {
        name: 'Resuscitace 2024',
        dateFrom: null,
        dateTo: null,
        info: '',
        location: null,
        participants: ['John Doe', 'Jane Doe']
      }
    ]}/>
    <h2>Konference ČR - B</h2>
    <ConferencesTable conferences={[
      {
        name: '16. ČESKO-SLOVENSKÝ KONGRES DĚTSKÉ ANESTEZIE, INTENZIVNÍ A URGENTNÍ MEDICÍNY',
        dateFrom: toDay('2023-11-23'),
        dateTo: toDay('2023-11-25'),
        info: 'Nějaký další text a pak odkaz: https://mhconsulting.cz/kongres/161-16-cesko-slovensky-kongres-detske-anestezie-intenzivni-a-urgentni-mediciny-23-25-listopadu-2023/obecne-informace\n',
        location: 'Mikulov',
        participants: ['John Doe', 'Jane Doe']
      }
    ]}/>
  </>)
}
