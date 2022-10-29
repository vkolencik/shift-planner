import { getToken } from './helpers/auth'
import { getHeaders } from './helpers/getHeaders'

const tenant = 'tenant1'

describe('requirements resource should', () => {
  it('return unauthorized when auth token is not present in the request', async () => {
    const resp = await fetch(`${backendBaseUrl}/api/${tenant}/users/${testUserId}/requirements/2022/10`)
    expect(resp.status).toEqual(401)
  })

  it('return unauthorized when asking for requirements of a different user', async () => {
    const token: string = await getToken()
    const resp = await fetch(`${backendBaseUrl}/api/${tenant}/users/some-different-user-id/requirements/2022/10`, getHeaders(token))

    expect(resp.status).toEqual(401)
  })

  it('return requirements for a given month', async () => {
    const token: string = await getToken()
    const resp = await fetch(`${backendBaseUrl}/api/${tenant}/users/${testUserId}/requirements/2022/10`, getHeaders(token))

    expect(resp.status).toEqual(200)
    expect(await resp.json()).toEqual({
      '2022-12-06': 'soft-yes',
      '2022-12-08': 'soft-yes',
      '2022-12-21': 'leave',
      '2022-12-13': 'hard-no'
    })
  })
})
