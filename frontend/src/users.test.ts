import { isExternalUser } from './users'
import { User } from './service/api'

describe('user utility functions should', () =>{
  describe(isExternalUser.name, () => {
    it('recognize external user by username', () => {
      const user: User = { userId: '1234', userName: 'Externista abc'}
      expect(isExternalUser(user)).toEqual(true)
    })

    it('recognize internal user by username', () => {
      const user: User = { userId: '1234', userName: 'John Doe'}
      expect(isExternalUser(user)).toEqual(false)
    })
  })
})
