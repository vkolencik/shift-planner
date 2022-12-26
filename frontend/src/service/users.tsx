import React, { createContext, PropsWithChildren, useContext, useEffect, useState } from 'react'
import { useApi, User } from './api'
import { Permission, usePermissions } from '../auth/auth'
import { SpinnerProps } from 'react-bootstrap'
import { LoadingSpinner } from '../components/LoadingSpinner'

const UserListContext = createContext<User[] | null>(null)

export const useUserList = (): User[] => {
  const users = useContext(UserListContext)

  if (users === null) {
    throw new Error('User list not set up.')
  }

  return users
}

export const WithUserList: React.FC<PropsWithChildren> = ({ children }) => {
  const api = useApi()
  const permissions = usePermissions()
  const canSeeUsers = () => permissions.includes(Permission.readRequirementsOfOthers)
  const [users, setUsers] = useState<User[] | null>(null)
  useEffect(() => {
      if (canSeeUsers()) {
        api.users.get().then(setUsers)
      }
    },
    [])

  if (!canSeeUsers()) {
    return <>{children}</>
  }

  if (users === null) {
    return <LoadingSpinner/>
  }
  
  return <UserListContext.Provider value={users}>{children}</UserListContext.Provider>
}
