import React, { useEffect } from 'react'
import './App.css'
import { Container } from 'react-bootstrap'
import { Route, Routes, useNavigate } from 'react-router-dom'
import { Requirements } from './pages/Requirements'
import { NavHeader } from './NavHeader'
import { Permission, usePermissions } from './auth/auth'
import { Approvals } from './pages/Approvals'
import { MonthlyOverview } from './pages/MonthlyOverview'
import { MonthSchedule } from './pages/MonthSchedule'
import { WeeklyOverview } from './pages/WeeklyOverview'
import { MonthScheduleEdit } from './pages/MonthScheduleEdit'
import { ToastContainer } from 'react-toastify'
import 'react-toastify/dist/ReactToastify.css'
import { Leaves } from './pages/Leaves'
import { Test } from './Test'
import { Conferences } from './Conferences'

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
    <>
      <ToastContainer
        position="top-center"
        autoClose={5000}
        hideProgressBar={false}
        newestOnTop={false}
        closeOnClick
        rtl={false}
        pauseOnFocusLoss
        draggable
        pauseOnHover
        theme="light"
      />

      <Container fluid={true} className="d-flex flex-column" style={{ height: '100vh' }}>

        <NavHeader/>

        <div className='overflow-auto flex-grow-1' style={{flex: '1'}}>
          <Routes>
            <Route path="/" element={<h1>Home</h1>}/>
            <Route path="/pozadavky" element={<Requirements/>}/>
            <Route path="/tydenik" element={<WeeklyOverview/>}/>
            <Route path="/mesicnik" element={<MonthlyOverview/>}/>
            <Route path="/schvalovani" element={<Approvals/>}/>
            <Route path="/dovolene" element={<Leaves/>}/>
            <Route path="/sluzby" element={<MonthSchedule/>}/>
            <Route path="/sluzby/:year/:month/editace" element={<MonthScheduleEdit/>}/>
            <Route path="/konference" element={<Conferences/>}/>
            <Route path="/test" element={<Test/>}/>
          </Routes>
        </div>
      </Container>
    </>)
}

export default App
