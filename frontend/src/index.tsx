import React from 'react'
import ReactDOM from 'react-dom/client'
import './index.css'
import App from './App'
import reportWebVitals from './reportWebVitals'
import {Auth0Provider} from '@auth0/auth0-react'
import {BrowserRouter} from 'react-router-dom'

const root = ReactDOM.createRoot(
  document.getElementById('root') as HTMLElement
)
root.render(
  <React.StrictMode>
    <BrowserRouter>
      <Auth0Provider
        domain="krtek.eu.auth0.com"
        clientId="DJath43A1oSS9KeuPbr0x5PiSG57ZcEv"
        redirectUri={window.location.origin}
        audience="https://b.krtek.me"
        cacheLocation='localstorage'
        scope="">
        <App/>
      </Auth0Provider>
    </BrowserRouter>
  </React.StrictMode>
)

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals()
