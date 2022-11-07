import React, { FC } from 'react'
import { useAuth0 } from '@auth0/auth0-react'
import { Button, Container, Nav, Navbar } from 'react-bootstrap'
import { UserBadge } from './UserBadge'
import { LinkContainer } from 'react-router-bootstrap'

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
          <LinkContainer to="/pozadavky"><Nav.Link>Požadavky</Nav.Link></LinkContainer>
          <LinkContainer to="/tydenik"><Nav.Link>Týdeník</Nav.Link></LinkContainer>
          <LinkContainer to="/schvalovani"><Nav.Link>Schvalování</Nav.Link></LinkContainer>
          <LinkContainer to="/sluzby"><Nav.Link>Služby</Nav.Link></LinkContainer>
          <LinkContainer to="/konference"><Nav.Link>Konference</Nav.Link></LinkContainer>
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
