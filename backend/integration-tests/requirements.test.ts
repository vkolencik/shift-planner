import { getToken } from './helpers/auth'
import { getHeaders } from './helpers/getHeaders'
import { putRequirement, RequirementParams } from './helpers/api'
import moment from 'moment'

describe('users requirements resource should', () => {
  let token: string
  let token2: string
  const testTenant = `test-tenant-requirements-${new Date().getTime()}`

  const activeMonthDate = moment().add(2, 'months')

  const activeYear = activeMonthDate.year()
  const activeMonthNumber = activeMonthDate.month() + 1
  const activeMonth = (activeMonthNumber < 10 ? '0' : '') + activeMonthNumber

  const requirementParams: RequirementParams = {
    tenant: testTenant,
    userId: testUserId,
    year: activeYear,
    month: activeMonthNumber,
    day: 6,
    requirementType: 'soft-yes'
  }

  beforeAll(async () => {
    token = await getToken('testUser1')
    token2 = await getToken('testUser2')
  })

  it('return unauthorized when auth token is not present in the request', async () => {
    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUserId)}/requirements/${activeYear}/${activeMonth}`)
    expect(resp.status).toEqual(401)
  })

  it('return forbidden when asking for requirements of a different user', async () => {
    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/some-different-user-id/requirements/${activeYear}/${activeMonth}`, getHeaders(token2))

    expect(resp.status).toEqual(403)
  })

  it('return empty array of requirements when none were created', async () => {
    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUserId)}/requirements/${activeYear}/${activeMonth}`, getHeaders(token))
    expect(resp.status).toEqual(200)
    expect(await resp.json()).toEqual({})
  })

  it('return unauthorized when putting new requirement without auth header', async () => {
    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUserId)}/requirements/${activeYear}/${activeMonth}/${activeMonth}`, {
      headers: {},
      method: 'put'
    })

    expect(resp.status).toEqual(401)
  })

  it('return forbidden when putting new requirement for different user', async () => {
    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUserId)}/requirements/${activeYear}/${activeMonth}/${activeMonth}`, {
      ...getHeaders(token2),
      method: 'put'
    })

    expect(resp.status).toEqual(403)
  })

  it('put new requirement', async () => {
    await putRequirement(requirementParams, token)

    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUserId)}/requirements/${activeYear}/${activeMonth}`, getHeaders(token))

    expect(resp.status).toEqual(200)
    expect(await resp.json()).toEqual({
      [`${activeYear}-${activeMonth}-06`]: 'soft-yes'
    })
  })

  it('only return requirements for the specified month', async () => {
    await putRequirement({ ...requirementParams, year: activeYear, month: activeMonthNumber, day: 6 }, token)
    await putRequirement({ ...requirementParams, year: activeYear, month: activeMonthNumber - 1 }, token)

    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUserId)}/requirements/${activeYear}/${activeMonth}`, getHeaders(token))

    expect(resp.status).toEqual(200)
    expect(await resp.json()).toEqual({
      [`${activeYear}-${activeMonth}-06`]: 'soft-yes'
    })
  })

  it('put new requirement to different user without influencing other users', async () => {
    await putRequirement({ ...requirementParams, userId: testUserId, requirementType: 'hard-no' }, token)
    await putRequirement({ ...requirementParams, userId: testUser2Id, requirementType: 'soft-yes' }, token)

    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUserId)}/requirements/${activeYear}/${activeMonth}`, getHeaders(token))

    expect(resp.status).toEqual(200)
    expect(await resp.json()).toEqual({
      [`${activeYear}-${activeMonth}-06`]: 'hard-no'
    })
  })

  it('put new requirement to different tenant without influencing original tenant', async () => {
    await putRequirement({ ...requirementParams, tenant: testTenant, requirementType: 'hard-no' }, token)
    await putRequirement({ ...requirementParams, tenant: testTenant + '-2', requirementType: 'soft-yes' }, token)

    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUserId)}/requirements/${activeYear}/${activeMonth}`, getHeaders(token))

    expect(resp.status).toEqual(200)
    expect(await resp.json()).toEqual({
      [`${activeYear}-${activeMonth}-06`]: 'hard-no'
    })
  })

  it('return unauthorized when deleting new requirement without auth header', async () => {
    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUserId)}/requirements/${activeYear}/${activeMonth}/06`, {
        method: 'delete'
      }
    )

    expect(resp.status).toEqual(401)
  })

  it('return forbidden when deleting new requirement for different user', async () => {
    await putRequirement(requirementParams, token)

    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUserId)}/requirements/${activeYear}/${activeMonth}/06`, {
      method: 'delete',
      ...getHeaders(token2)
    })

    expect(resp.status).toEqual(403)
  })

  it('return not found when deleting nonexistent requirement', async () => {
    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUserId)}/requirements/2019/${activeMonth}/13`, {
      method: 'delete',
      ...getHeaders(token)
    })

    expect(resp.status).toEqual(404)
  })

  it('delete a requirement', async () => {
    await putRequirement(requirementParams, token)

    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUserId)}/requirements/${activeYear}/${activeMonth}/06`, {
      method: 'delete',
      ...getHeaders(token)
    })

    expect(resp.status).toEqual(200)

    const getResp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUserId)}/requirements/${activeYear}/${activeMonth}`, getHeaders(token))

    const requirements = await getResp.json()

    expect(requirements[`${activeYear}-${activeMonth}-06`]).toBeFalsy()
  })

  it('not allow adding requirement in the past for normal user', async () => {
    const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUser2Id)}/requirements/2020/01/01`, {
      ...getHeaders(token2),
      method: 'put',
      body: JSON.stringify({ requirementType: 'hard-no' })
    })

    expect(resp.status).toEqual(403)
  })

  describe('give access to another user\'s requirements to manager when', () => {
    it('deleting other user\'s requirement when deleted by manager', async () => {
      await putRequirement({ ...requirementParams, userId: testUser2Id }, token2)

      const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUser2Id)}/requirements/${activeYear}/${activeMonth}/06`, {
        method: 'delete',
        ...getHeaders(token)
      })

      expect(resp.status).toEqual(200)

      const getResp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUser2Id)}/requirements/${activeYear}/${activeMonth}`, getHeaders(token))

      const requirements = await getResp.json()

      expect(requirements[`${activeYear}-${activeMonth}-06`]).toBeFalsy()
    })

    it('getting list of requirements', async () => {
      await putRequirement({ ...requirementParams, userId: testUser2Id }, token)

      const resp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUser2Id)}/requirements/${activeYear}/${activeMonth}`, getHeaders(token))

      expect(resp.status).toEqual(200)
      expect(await resp.json()).toEqual({
        [`${activeYear}-${activeMonth}-06`]: 'soft-yes'
      })
    })

    it('creating requirement for someone else', async () => {
      const resp = await fetch(
        `${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUser2Id)}/requirements/${activeYear}/${activeMonth}/08`, {
          ...getHeaders(token),
          method: 'put',
          body: JSON.stringify({ requirementType: 'hard-no' })
        })

      expect(resp.status).toEqual(200)

      const getResp = await fetch(`${backendBaseUrl}/api/${testTenant}/users/${encodeURIComponent(testUser2Id)}/requirements/${activeYear}/${activeMonth}`, getHeaders(token))

      expect(getResp.status).toEqual(200)
      expect(await getResp.json()).toEqual(expect.objectContaining({
        [`${activeYear}-${activeMonth}-08`]: 'hard-no'
      }))
    })
  })
})
