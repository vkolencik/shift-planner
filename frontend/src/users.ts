import { User } from './service/api'

export function isExternalUser (u: User) {
  return u.userName.toLowerCase().startsWith('externista')
}
