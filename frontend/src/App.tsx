import React, { useEffect } from 'react'
import './App.css'
import { Col, Container, Row } from 'react-bootstrap'
import { Route, Routes, useNavigate } from 'react-router-dom'
import { Requirements } from './Requirements'
import { NavHeader } from './NavHeader'
import { Footer } from './Footer'
import { Permission, usePermissions } from './auth/auth'
import { Approvals } from './Approvals'

function App () {
  const navigate = useNavigate()
  const permissions = usePermissions()

  useEffect(() => {
    const defaultPath = permissions.includes(Permission.readRequirementsOfOthers) ? 'schvalovani' : 'pozadavky'
    if (window.location.pathname === '/') {
      navigate(defaultPath)
    }
  }, [])

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
              <Route path="/schvalovani" element={<Approvals/>}/>
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
