import { getToken } from './helpers/auth'
import { getHeaders } from './helpers/getHeaders'
import { putRequirement, RequirementParams } from './helpers/api'

describe('approval workflow', () => {
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

  describe('requirements overview endpoint should', () => {
    it('return unauthorized when auth token is not present in the request', async () => {
      const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/requirements/2022/12`)
      expect(resp.status).toEqual(401)
    })

    it('return unauthorized when the user doesn\'t have permissions', async () => {
      const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/requirements/2022/12`, getHeaders(token2))
      expect(resp.status).toEqual(403)
    })

    it('return requirements from all users', async () => {
      await putRequirement({ ...requirementParams, requirementType: 'hard-no', userId: testUserId }, token)
      await putRequirement({ ...requirementParams, requirementType: 'soft-yes', userId: testUser2Id }, token2)

      const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/requirements/2022/12`, getHeaders(token))
      expect(resp.status).toEqual(200)

      expect(resp.status).toEqual(200)
      const requirements = await resp.json()
      expect(requirements).toHaveLength(2)
      expect(requirements).toEqual(expect.arrayContaining([
        { userId: testUserId, userName: testUserName, date: '2022-12-06', requirementType: 'hard-no' },
        { userId: testUser2Id, userName: testUser2Name, date: '2022-12-06', requirementType: 'soft-yes' }
      ]))
    })
  })

  describe('approval endpoint should', () => {
    it('approve requirement', async () => {
      await putRequirement({ ...requirementParams, requirementType: 'leave', userId: testUser2Id }, token2)

      const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${testUser2Id}/requirements/2022/12/06/approval`, {
        ...getHeaders(token),
        method: 'put',
        body: JSON.stringify({ approved: true })
      })

      expect(resp.status).toEqual(200)

      const getResp = await fetch(`${backendBaseUrl}/api/${testTenant}/requirements/2022/12`, getHeaders(token))
      const requirements = await getResp.json()
      expect(requirements).toContain(expect.objectContaining({
        date: '2022-12-06',
        userId: testUser2Id,
        requirementType: 'leave',
        approved: true
      }))
    })

    it('return bad request when requirement is not leave', async () => {
      await putRequirement({ ...requirementParams, requirementType: 'hard-no', userId: testUser2Id }, token2)

      const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${testUser2Id}/requirements/2022/12/06/approval`, {
        ...getHeaders(token),
        method: 'put',
        body: JSON.stringify({ approved: true })
      })

      expect(resp.status).toEqual(400)
    })

    it('return not found when requirement does not exist', async () => {
      const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${testUser2Id}/requirements/2022/01/06/approval`, {
        ...getHeaders(token),
        method: 'put',
        body: JSON.stringify({ approved: true })
      })

      expect(resp.status).toEqual(404)
    })

    it('return unauthorized when auth token is not present in the request', async () => {
      const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${testUser2Id}/requirements/2022/12/06/approval`, {
        method: 'put',
        body: JSON.stringify({ approved: true })
      })

      expect(resp.status).toEqual(401)
    })

    it('return forbidden when user is not admin', async () => {
      const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${testUser2Id}/requirements/2022/12/06/approval`, {
        ...getHeaders(token2),
        method: 'put',
        body: JSON.stringify({ approved: true })
      })

      expect(resp.status).toEqual(403)
    })
  })
})
