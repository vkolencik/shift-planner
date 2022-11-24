import React, {FC} from 'react'
import {useAuth0} from '@auth0/auth0-react'

export const UserBadge: FC = () => {
  const {user} = useAuth0()

  if (!user) {
    throw new Error('No user logged in')
  }

  return (<span>
    <img
      src={user.picture}
      alt={user.name}
      className="rounded-circle img-fluid"
      style={{maxWidth: '32px'}}
    /><span className="d-md-inline d-none">&nbsp;{user.name}</span>
  </span>)
}
