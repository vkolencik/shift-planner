import { getToken } from './helpers/auth'
import { getHeaders } from './helpers/getHeaders'

describe('Users endpoint should', () => {
  const testTenant = `test-tenant-requirements-${new Date().getTime()}`

  let token
  let token2
  beforeAll(async () => {
    token = await getToken('testUser1')
    token2 = await getToken('testUser2')
  })

  it('return list of users', async () => {
    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/`, getHeaders(token))

    expect(resp.status).toEqual(200)

    const users = await resp.json()

    expect(users).toEqual(expect.arrayContaining([
      {
        userId: testUserId,
        userName: testUserName
      },
      {
        userId: testUser2Id,
        userName: testUser2Name
      },
      {
        userId: deactivatedUserId,
        userName: deactivatedUserName,
        activeUntil: deactivatedUserActiveUntilMonth
      }
    ]))
  })

  it('return unauthorized when auth token is not present', async () => {
    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/`)

    expect(resp.status).toEqual(401)
  })
})
