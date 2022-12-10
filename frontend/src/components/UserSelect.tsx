import { Typeahead } from 'react-bootstrap-typeahead'
import { useUserList } from '../service/users'
import React, { useEffect, useState } from 'react'
import { Option } from 'react-bootstrap-typeahead/types/types'
import 'react-bootstrap-typeahead/css/Typeahead.css'
import { User } from '../service/api'

type Props = {
  autofocus?: boolean
  initialUserId?: string | null
  onUserSelected: (user: User) => void
}

export const UserSelect: React.FC<Props> = (props: Props) => {
  const users = useUserList()
  const [selectedUsers, setSelectedUsers] = useState<Option[]>()
  const options = users.map(u => ({ id: u.userId, label: u.userName }))

  useEffect(() => {
    if (props.initialUserId) {
      const userOption = options.find(o => o.id === props.initialUserId)

      if (userOption === undefined) {
        throw new Error(`Selected user ID '${props.initialUserId}' not found within options`)
      }

      setSelectedUsers([userOption])
    }
  }, [props.initialUserId])

  const handleUserSelected = (userOptions: Option[]) => {
    setSelectedUsers(userOptions)

    if (userOptions.length !== 1) {
      return
    }

    // @ts-ignore
    const selectedUserId = userOptions[0]['id'] as string
    const user = users.find(u => u.userId === selectedUserId)

    if (user === undefined) {
      throw new Error(`Selected user ID ${selectedUserId} not found`)
    }

    props.onUserSelected(user)
  }

  return (<Typeahead
    options={options}
    selected={selectedUsers}
    clearButton
    autoFocus={props.autofocus}
    highlightOnlyResult
    onChange={handleUserSelected}
    id='user-select'
  />)
}
