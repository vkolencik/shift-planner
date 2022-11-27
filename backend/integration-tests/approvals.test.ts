import { getToken } from './helpers/auth'
import { getHeaders } from './helpers/getHeaders'
import { putRequirement, RequirementParams } from './helpers/api'

describe('requirements overview endpoint should', () => {
  let token: string
  let token2: string

  const testTenant = `test-tenant-req-overview-${new Date().getTime()}`

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
    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/requirements/2022/12`)
    expect(resp.status).toEqual(401)
  })

  it('return unauthorized when the user doesn\'t have permissions', async () => {
    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/requirements/2022/12`, getHeaders(token2))
    expect(resp.status).toEqual(403)
  })

  it('return requirements from all users', async () => {
    await putRequirement({...requirementParams, requirementType: 'hard-no', userId: testUserId}, token)
    await putRequirement({...requirementParams, requirementType: 'soft-yes', userId: testUser2Id}, token2)

    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/requirements/2022/12`, getHeaders(token))
    expect(resp.status).toEqual(200)

    expect(resp.status).toEqual(200)
    const requirements = await resp.json()
    expect(requirements).toHaveLength(2)
    expect(requirements).toEqual(expect.arrayContaining([
      {userId: testUserId, userName: testUserName,  date: '2022-12-06', requirementType: 'hard-no'},
      {userId: testUser2Id, userName: testUser2Name, date: '2022-12-06', requirementType: 'soft-yes'}
    ]))
  })
})
