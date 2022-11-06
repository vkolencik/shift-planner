import { getToken } from './helpers/auth'
import { getHeaders } from './helpers/getHeaders'

describe('requirements resource should', () => {
  let token: string
  const testTenant = `test-tenant-${new Date().getTime()}`

  async function putRequirement (tenant: string, userId: string, year: number, month: number, day: number, requirementType: string): Promise<Response> {
    const resp = await fetch(`${backendBaseUrl}/api/${tenant}/users/${encodeURIComponent(userId)}/requirements/${year}/${String(month).padStart(2, '0')}/${String(day).padStart(2, '0')}`, {
      ...getHeaders(token),
      method: 'put',
      body: JSON.stringify({ requirementType: requirementType })
    })
    expect(resp.status).toEqual(200)

    return resp
  }

  beforeAll(async () => {
    token = await getToken()
  })

  it('return unauthorized when auth token is not present in the request', async () => {
    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUserId)}/requirements/2022/10`)
    expect(resp.status).toEqual(401)
  })

  it('return unauthorized when asking for requirements of a different user', async () => {
    const token: string = await getToken()
    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/some-different-user-id/requirements/2022/10`, getHeaders(token))

    expect(resp.status).toEqual(401)
  })

  it('return empty array of requirements when none were created', async () => {
    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUserId)}/requirements/2022/10`, getHeaders(token))
    expect(resp.status).toEqual(200)
    expect(await resp.json()).toEqual({})
  })

  it('put new requirement', async () => {
    await putRequirement(testTenant, testUserId, 2012, 12, 6, 'soft-yes')

    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUserId)}/requirements/2022/10`, getHeaders(token))

    expect(resp.status).toEqual(200)
    expect(await resp.json()).toEqual({
      '2022-12-06': 'soft-yes'
    })
  })
  it('return unauthorized when putting new requirement without auth header', async () => {
    await putRequirement(testTenant, testUserId, 2012, 12, 6, 'soft-yes')

    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUserId)}/requirements/2022/10`)

    expect(resp.status).toEqual(401)
  })

  it('return unauthorized when putting new requirement for different user', async () => {
    await putRequirement(testTenant, testUserId, 2012, 12, 6, 'soft-yes')

    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/some-different-user/requirements/2022/10`, getHeaders(token))

    expect(resp.status).toEqual(401)
  })

  it('put new requirement to different tenant without influencing original tenant', async () => {
    await putRequirement(`${testTenant}`, testUserId, 2012, 12, 6, 'hard-no')
    await putRequirement(`${testTenant}-2`, testUserId, 2012, 12, 6, 'soft-yes')

    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUserId)}/requirements/2022/10`, getHeaders(token))

    expect(resp.status).toEqual(200)
    expect(await resp.json()).toEqual({
      '2022-12-06': 'hard-no'
    })
  })

})
