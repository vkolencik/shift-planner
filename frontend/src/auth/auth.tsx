import React, { useEffect, useState } from 'react'
import { createContext, PropsWithChildren, ReactNode, useContext } from 'react'
import { useAuth0 } from '@auth0/auth0-react'
import Button from 'react-bootstrap/Button'
import { LoadingSpinner } from '../components/LoadingSpinner'

const PermissionsContext = createContext<Permission[] | null>(null)

export enum Permission {
  readRequirementsOfOthers = 'read:requirements_of_others',
  approveRequirementsOfOthers = 'approve:requirements_of_others'
}

export const usePermissions = (): Permission[] => {
  const permissions = useContext(PermissionsContext)
  if (permissions === null) {
    throw new Error('Permissions context not found. Outside <WithAuth> element?')
  }

  return permissions
}

export const WithAuth: React.FC<PropsWithChildren> = ({ children }) => {
  const { loginWithRedirect, isAuthenticated, getIdTokenClaims } = useAuth0()
  const [permissions, setPermissions] = useState<Permission[] | null>(null)

  useEffect(() => {
    if (!isAuthenticated) {
      setPermissions(null)
      return
    }

    if (permissions === null) {
      getIdTokenClaims()
        .then(c => {
          const loadedPermissions = c?.user_authorization?.permissions

          if (loadedPermissions === null || loadedPermissions === undefined) {
            // TODO: remove this fallback for backward compatibility:
            setPermissions([])
          }

          if (loadedPermissions === null || loadedPermissions === undefined || !Array.isArray(loadedPermissions)) {
            throw new Error('Cannot load user permissions')
          }

          const invalidPermissions = loadedPermissions.filter(p => !Object.values(Permission).includes(p))
          if (invalidPermissions.length > 0) {
            throw new Error(`Unknown permissions found: ${invalidPermissions}`)
          }

          setPermissions(loadedPermissions)
        })
    }
  }, [permissions, isAuthenticated])

  if (!isAuthenticated) {
    return <div className="d-flex flex-column min-vh-100 h-100 d-flex align-items-center justify-content-center">
      <img
        style={{height: '70vh', maxHeight: '400px'}}
        src='https://krtek-assets-720500131615.s3.eu-central-1.amazonaws.com/krtek1.svg'/>
      <Button onClick={() => loginWithRedirect()}>Login</Button>
    </div>
  }

  if (permissions === null) {
    return <LoadingSpinner/>
  }

  return <PermissionsContext.Provider value={permissions}>
    {children}
  </PermissionsContext.Provider>
}
