import {getToken} from './helpers/auth'
import {getHeaders} from './helpers/getHeaders'
import { putRequirement, RequirementParams } from './helpers/api'

describe('users requirements resource should', () => {
  let token: string
  let token2: string
  const testTenant = `test-tenant-${new Date().getTime()}`

  const requirementParams: RequirementParams = {
    tenant: testTenant,
    userId: testUserId,
    year: 2022,
    month: 12,
    day: 6,
    requirementType: 'soft-yes'
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
    await putRequirement(requirementParams, token)

    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUserId)}/requirements/2022/12`, getHeaders(token))

    expect(resp.status).toEqual(200)
    expect(await resp.json()).toEqual({
      '2022-12-06': 'soft-yes'
    })
  })

  it('only return requirements for the specified month', async () => {
    await putRequirement({...requirementParams, year: 2022, month: 12, day: 6}, token)
    await putRequirement({...requirementParams, year: 2022, month: 11}, token)

    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUserId)}/requirements/2022/12`, getHeaders(token))

    expect(resp.status).toEqual(200)
    expect(await resp.json()).toEqual({
      '2022-12-06': 'soft-yes'
    })
  })

  it('put new requirement to different user without influencing other users', async () => {
    await putRequirement({ ...requirementParams, userId: testUserId, requirementType: 'hard-no' }, token)
    await putRequirement({ ...requirementParams, userId: testUser2Id, requirementType: 'soft-yes' }, token2)

    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUserId)}/requirements/2022/12`, getHeaders(token))

    expect(resp.status).toEqual(200)
    expect(await resp.json()).toEqual({
      '2022-12-06': 'hard-no'
    })
  })

  it('put new requirement to different tenant without influencing original tenant', async () => {
    await putRequirement({ ...requirementParams, tenant: testTenant, requirementType: 'hard-no' }, token)
    await putRequirement({ ...requirementParams, tenant: testTenant + '-2', requirementType: 'soft-yes' }, token)

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
    await putRequirement(requirementParams, token)

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
    await putRequirement(requirementParams, token)

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
