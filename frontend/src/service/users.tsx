import React, { createContext, PropsWithChildren, useContext, useEffect, useState } from 'react'
import { useApi, UserWAD } from './api'
import { usePermissions } from '../auth/auth'
import { LoadingSpinner } from '../components/LoadingSpinner'

const UserListContext = createContext<UserWAD[] | null>(null)

export const useUserList = (): UserWAD[] => {
  const users = useContext(UserListContext)

  if (users === null) {
    throw new Error('User list not set up.')
  }

  return users
}

export const WithUserList: React.FC<PropsWithChildren> = ({ children }) => {
  const api = useApi()
  const permissions = usePermissions()
  const [users, setUsers] = useState<UserWAD[] | null>(null)
  useEffect(() => {
      api.users.get().then(setUsers)
    },
    [])

  if (users === null) {
    return <LoadingSpinner/>
  }

  return <UserListContext.Provider value={users}>{children}</UserListContext.Provider>
}
