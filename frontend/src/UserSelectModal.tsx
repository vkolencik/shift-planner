import React from 'react'
import { User } from './service/api'
import Modal from 'react-bootstrap/Modal'
import { UserSelect } from './components/UserSelect'

export const UserSelectModal: React.FC<{ isOpen: boolean, onHide: () => void, onUserSelected: (user: User) => void }> = (props) =>
  <Modal
    show={props.isOpen}
    onHide={props.onHide}
    key="user-select-modal"
  >
    <Modal.Header closeButton>Výběr uživatele</Modal.Header>
    <Modal.Body>
      <UserSelect onUserSelected={props.onUserSelected} autofocus={true}/>
    </Modal.Body>
  </Modal>
