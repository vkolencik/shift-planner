import React, { FC, useEffect, useState } from 'react'
import './App.css'
import { useAuth0 } from '@auth0/auth0-react'
import Button from 'react-bootstrap/Button'
import { Col, Container, Nav, Navbar, Row } from 'react-bootstrap'
import { Link, Route, Routes } from 'react-router-dom'
import { Requirements } from './Requirements'

const baseUrl = 'https://b.krtek.me'
// const baseUrl = 'http://localhost:65041'

const ValuesFromBackend: React.FC = () => {
  const { user, getAccessTokenSilently, isAuthenticated } = useAuth0()
  const [values, setValues] = useState<string[] | null>(null)
  // const [userMetadata, setUserMetadata] = useState<any | null>(null)

  useEffect(() => {
    if (user === undefined || !isAuthenticated) {
      setValues(null)
      return
    }

    getAccessTokenSilently({
      audience: 'https://b.krtek.me',
      scope: '',
    }).then(token => fetch(baseUrl + '/api/values', {
      headers: {
        Authorization: `Bearer ${token}`,
      }
    }))
      .then((response) => response.json())
      .then((data) => setValues(data))
  }, [getAccessTokenSilently, user, user?.sub])

  if (values === null) {
    return <div>Please log in</div>
  }

  return <ul>
    {values.map(v => <li>{v}</li>)}
  </ul>

  // if (userMetadata === null) {
  //   return <>Cannot fetch metadata</>
  // }

  // return <pre>{JSON.stringify(userMetadata, null, 2)}</pre>
}

const UserBadge: FC = () => {
  const { loginWithRedirect, isAuthenticated, logout, user } = useAuth0()

  if (!user) {
    throw new Error('No user logged in')
  }

  return (<span><img src={user.picture} alt={user.name} className='rounded-circle img-fluid' style={{maxWidth: '32px'}} /><span className='d-md-inline d-none'>&nbsp;{user.name}</span></span>)
}

const NavHeader: FC = () => {
  const { loginWithRedirect, isAuthenticated, logout, user } = useAuth0()

  return (<Navbar bg="light" expand="sm" sticky="top" className="border-bottom">
    <Container>
      <Navbar.Brand href="/" className="navbar-brand basic-navbar-nav"><img className='rounded-circle img-fluid' width='64' height='64'  src='https://krtek-assets-720500131615.s3.eu-central-1.amazonaws.com/mole2.svg'/></Navbar.Brand>
      <Navbar.Toggle aria-controls="basic-navbar-nav"/>
      <Navbar.Collapse id="basic-navbar-nav">
        <Nav className="me-auto">
          <Nav.Link><Link to='/pozadavky'>Požadavky</Link></Nav.Link>
          <Nav.Link><Link to="/tydenik">Týdeník</Link></Nav.Link>
          <Nav.Link><Link to="/schvalovani">Schvalování</Link></Nav.Link>
          <Nav.Link><Link to="/sluzby">Služby</Link></Nav.Link>
          <Nav.Link><Link to="/konference">Konference</Link></Nav.Link>
        </Nav>
      </Navbar.Collapse>
      <UserBadge/>
      {/*<Button*/}
      {/*  onClick={() => logout({returnTo: window.location.protocol + '//' + window.location.host})} className='ms-3'>*/}
      {/*  Logout*/}
      {/*</Button>*/}
    </Container>
  </Navbar>)
}

const Footer: FC = () =>
  <footer className="footer pl-3 text-bg-light w-100 d-none d-sm-block mt-auto sticky-bottom border-top">
    <Container fluid color="secondary">
      <Row>
        <Col fluid className="border-end col-auto">
          <a href="https://krtek.me/info">
            O aplikaci
          </a>
        </Col>
        <Col>
          V případě problémů nás prosím kontaktujte na <a href="mailto:help@krtek.me">help@krtek.me</a>
        </Col>
      </Row>
    </Container>
  </footer>

function App () {
  const { loginWithRedirect, isAuthenticated, logout, user, isLoading, getAccessTokenSilently} = useAuth0()

  useEffect(() => {
    if (!isLoading)
      getAccessTokenSilently()
  }, [isLoading, getAccessTokenSilently])

  if (!isAuthenticated) {
    return <div className="d-flex flex-column min-vh-100 h-100 d-flex align-items-center justify-content-center">
        <Button onClick={() => loginWithRedirect()}>Login</Button>
    </div>
  }

  return (
    <div className="d-flex flex-column min-vh-100">
      <NavHeader/>
      <Container fluid>
        <Row>
          <Col xs="12">
            <Routes>
              <Route path="/" element={<h1>Home</h1>}/>
              <Route path="/pozadavky" element={<Requirements />}/>
              <Route path="/tydenik" element={<h1>Týdeník</h1>}/>
              <Route path="/schvalovani" element={<h1>Schvalování</h1>}/>
              <Route path="/sluzby" element={<h1>Služby</h1>}/>
              <Route path="/konference" element={<h1>Konference</h1>}/>
            </Routes>
          </Col>
        </Row>
      </Container>
      <Footer/>
    </div>


    /*<div className="App">
      <header className="App-header">
        { !isAuthenticated && <Button onClick={() => loginWithRedirect()}>Login</Button> }

        { isAuthenticated && user !== undefined && (
          <>
            <Button onClick={() => logout({returnTo: window.location.href})}>Logout</Button>
            <div>
              <img src={user.picture} alt={user.name} />
              <h2>{user.name}</h2>
              <p>{user.email}</p>
            </div>
          <ValuesFromBackend />
          </>
        )}
      </header>
    </div>*/
  )
}

export default App
