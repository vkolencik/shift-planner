import {getToken} from './helpers/auth'
import {getHeaders} from './helpers/getHeaders'

describe('requirements resource should', () => {
  let token: string
  let token2: string
  const testTenant = `test-tenant-${new Date().getTime()}`

  async function putRequirement(tenant: string, userId: string, year: number, month: number, day: number, requirementType: string, authToken: string | null = null): Promise<Response> {
    if (authToken === null) {
      authToken = token
    }

    const resp = await fetch(`${backendBaseUrl}/api/${tenant}/users/${encodeURIComponent(userId)}/requirements/${year}/${String(month).padStart(2, '0')}/${String(day).padStart(2, '0')}`, {
      ...getHeaders(authToken),
      method: 'put',
      body: JSON.stringify({requirementType: requirementType})
    })
    expect(resp.status).toEqual(200)

    return resp
  }

  beforeAll(async () => {
    token = await getToken('testUser1')
    token2 = await getToken('testUser2')
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

  it('return unauthorized when putting new requirement without auth header', async () => {
    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUserId)}/requirements/2022/10/12`, {
      headers: {},
      method: 'put'
    })

    expect(resp.status).toEqual(401)
  })

  it('return unauthorized when putting new requirement for different user', async () => {
    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUserId)}/requirements/2022/10/12`, {
      ...getHeaders(token2),
      method: 'put'
    })

    expect(resp.status).toEqual(401)
  })

  it('put new requirement', async () => {
    await putRequirement(testTenant, testUserId, 2022, 12, 6, 'soft-yes')

    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUserId)}/requirements/2022/12`, getHeaders(token))

    expect(resp.status).toEqual(200)
    expect(await resp.json()).toEqual({
      '2022-12-06': 'soft-yes'
    })
  })

  it('only return requirements for the specified month', async () => {
    await putRequirement(testTenant, testUserId, 2022, 12, 6, 'soft-yes')
    await putRequirement(testTenant, testUserId, 2022, 11, 6, 'hard-no')

    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUserId)}/requirements/2022/12`, getHeaders(token))

    expect(resp.status).toEqual(200)
    expect(await resp.json()).toEqual({
      '2022-12-06': 'soft-yes'
    })
  })

  it('put new requirement to different user without influencing other users', async () => {
    await putRequirement(testTenant, testUserId, 2022, 12, 6, 'hard-no')
    await putRequirement(testTenant, testUser2Id, 2022, 12, 7, 'soft-yes', token2)

    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUserId)}/requirements/2022/12`, getHeaders(token))

    expect(resp.status).toEqual(200)
    expect(await resp.json()).toEqual({
      '2022-12-06': 'hard-no'
    })
  })

  it('put new requirement to different tenant without influencing original tenant', async () => {
    await putRequirement(`${testTenant}`, testUserId, 2022, 12, 6, 'hard-no')
    await putRequirement(`${testTenant}-2`, testUserId, 2022, 12, 7, 'soft-yes')

    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUserId)}/requirements/2022/12`, getHeaders(token))

    expect(resp.status).toEqual(200)
    expect(await resp.json()).toEqual({
      '2022-12-06': 'hard-no'
    })
  })

  it('return unauthorized when deleting new requirement without auth header', async () => {
    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUserId)}/requirements/2022/12/06`, {
        method: 'delete'
      }
    )

    expect(resp.status).toEqual(401)
  })

  it('return unauthorized when deleting new requirement for different user', async () => {
    await putRequirement(testTenant, testUserId, 2022, 12, 6, 'soft-yes', token)

    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUserId)}/requirements/2022/12/06`, {
      method: 'delete',
      ...getHeaders(token2)
    })

    expect(resp.status).toEqual(401)
  })

  it('return not found when deleting nonexistent requirement', async () => {
    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUserId)}/requirements/2019/12/13`, {
      method: 'delete',
      ...getHeaders(token)
    })

    expect(resp.status).toEqual(404)
  })

  it('delete a requirement', async () => {
    await putRequirement(testTenant, testUserId, 2022, 12, 6, 'soft-yes')

    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUserId)}/requirements/2022/12/06`, {
      method: 'delete',
      ...getHeaders(token)
    })

    expect(resp.status).toEqual(200)

    const getResp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUserId)}/requirements/2022/12`, getHeaders(token))

    const requirements = await getResp.json()

    expect(requirements['2022-12-06']).toBeFalsy()
  })
})
