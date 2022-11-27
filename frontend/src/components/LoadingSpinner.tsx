import React from 'react'
import { Spinner } from 'react-bootstrap'

export const LoadingSpinner: React.FC = () => (
  <div
    className="position-absolute top-0 start-0 w-100 h-100 d-flex flex-column align-items-center bg-white justify-content-center opacity-50"
    style={{ 'zIndex': 1000 }}>
    <Spinner animation="border"/>
  </div>
)
