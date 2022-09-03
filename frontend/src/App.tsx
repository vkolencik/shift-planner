import React, { useEffect, useState } from 'react'
import './App.css';
import { useAuth0 } from '@auth0/auth0-react'
import Button from 'react-bootstrap/Button';

const baseUrl = 'https://b.krtek.me'
// const baseUrl = 'https://localhost:65040'

const ValuesFromBackend: React.FC = () => {
  const [values, setValues] = useState<string[] | null>(null)
  useEffect(() => {
    fetch(baseUrl + '/api/values')
      .then((response) => response.json())
      .then((data) => setValues(data));
  }, [])

  if (values === null) {
    return <div>Please log in</div>
  }

  return <ul>
    {values.map(v => <li>{v}</li>)}
  </ul>
}

function App() {
  const { loginWithRedirect, isAuthenticated, logout, user } = useAuth0();

  return (
    <div className="App">
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
          </>
        )}

        <ValuesFromBackend />
      </header>
    </div>
  );
}

export default App;
