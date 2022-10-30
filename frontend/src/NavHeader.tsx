import React, { FC } from 'react'
import { useAuth0 } from '@auth0/auth0-react'
import { Button, Container, Nav, Navbar } from 'react-bootstrap'
import { Link } from 'react-router-dom'
import { UserBadge } from './UserBadge'

export const NavHeader: FC = () => {
  const { logout } = useAuth0()

  const displayLogoutButton = false // TODO: Enable once random logout issue on mobile is resolved

  return (<Navbar bg="light" expand="sm" sticky="top" className="border-bottom">
    <Container>
      <Navbar.Brand href="/" className="navbar-brand basic-navbar-nav">
        <img
          className="rounded-circle img-fluid"
          width="64" height="64"
          src="https://krtek-assets-720500131615.s3.eu-central-1.amazonaws.com/mole2.svg"
          alt='logo'/>
      </Navbar.Brand>
      <Navbar.Toggle aria-controls="basic-navbar-nav"/>
      <Navbar.Collapse id="basic-navbar-nav">
        <Nav className="me-auto">
          <Nav.Link><Link to="/pozadavky">Požadavky</Link></Nav.Link>
          <Nav.Link><Link to="/tydenik">Týdeník</Link></Nav.Link>
          <Nav.Link><Link to="/schvalovani">Schvalování</Link></Nav.Link>
          <Nav.Link><Link to="/sluzby">Služby</Link></Nav.Link>
          <Nav.Link><Link to="/konference">Konference</Link></Nav.Link>
        </Nav>
      </Navbar.Collapse>
      <UserBadge/>
      {displayLogoutButton &&
        <Button
          onClick={() => logout({ returnTo: window.location.protocol + '//' + window.location.host })} className='ms-3'>
          Logout
        </Button>
      }
    </Container>
  </Navbar>)
}
