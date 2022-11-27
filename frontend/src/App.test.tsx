import React from 'react';
import {render} from '@testing-library/react';
import App from './App';
import { BrowserRouter } from 'react-router-dom'
import { WithAuth } from './auth/auth'

test('renders the app', () => {
  render(<BrowserRouter><WithAuth><App/></WithAuth></BrowserRouter>)
});
