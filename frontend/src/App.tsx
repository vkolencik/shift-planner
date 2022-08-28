import React, { useEffect, useState } from 'react'
import logo from './logo.svg';
import './App.css';

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
    return <div>loading...</div>
  }

  return <ul>
    {values.map(v => <li>{v}</li>)}
  </ul>
}

function App() {
  return (
    <div className="App">
      <header className="App-header">
        <img src={logo} className="App-logo" alt="logo" />
        <p>
          Edit <code>src/App.tsx</code> and save to reload.
        </p>
        <a
          className="App-link"
          href="https://reactjs.org"
          target="_blank"
          rel="noopener noreferrer"
        >
          Learn React
        </a>
        <ValuesFromBackend />
      </header>
    </div>
  );
}

export default App;
