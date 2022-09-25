import React, { useEffect, useState } from 'react'
import './App.css';
import { useAuth0 } from '@auth0/auth0-react'
import Button from 'react-bootstrap/Button';

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
          <ValuesFromBackend />
          </>
        )}
      </header>
    </div>
  );
}

export default App;
