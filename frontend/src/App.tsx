import React, {useEffect} from 'react'
import './App.css'
import {useAuth0} from '@auth0/auth0-react'
import Button from 'react-bootstrap/Button'
import {Col, Container, Row} from 'react-bootstrap'
import {Route, Routes} from 'react-router-dom'
import {Requirements} from './Requirements'
import {NavHeader} from './NavHeader'
import {Footer} from './Footer'

function App() {
  const {loginWithRedirect, isAuthenticated, isLoading, getAccessTokenSilently} = useAuth0()

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
      <Container fluid={true}>
        <Row>
          <Col xs="12">
            <Routes>
              <Route path="/" element={<h1>Home</h1>}/>
              <Route path="/pozadavky" element={<Requirements/>}/>
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
  )
}

export default App
