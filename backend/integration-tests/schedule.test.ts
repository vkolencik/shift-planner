import { getToken } from './helpers/auth'
import { getHeaders } from './helpers/getHeaders'

describe('schedule endpoint should', () => {
  const tenant = 'fnb'
  let token
  let token2
  beforeAll(async () => {
    jest.setTimeout(20000)

    token = await getToken('testUser1')
    token2 = await getToken('testUser2')
  })

  it('load schedule', async () => {
    const resp = await fetch(`${backendBaseUrl}/api/${tenant}/schedule/2023/01`, getHeaders(token))

    expect(resp.status).toEqual(200)

    const responseData = await resp.json()
    const schedule = responseData.shifts

    expect(schedule.length).toEqual(31 * 6)

    const firstDayShifts = schedule.filter(s => s.day === '2023-01-01')
    expect(firstDayShifts.length).toEqual(6)
    expect(firstDayShifts.map(s => s.workplace)).toContainAllValues([
      'A1',
      'A2',
      'A3',
      'RES',
      'KPR',
      'URG'
    ])

    const sampleUser = firstDayShifts[0].user
    expect(sampleUser).toHaveProperty('userId', expect.not.toBeEmpty())
    expect(sampleUser).toHaveProperty('userName', expect.not.toBeEmpty())
  })

  it('return no content for a month that doesn\'t have schedule', async () => {
    const resp = await fetch(`${backendBaseUrl}/api/${tenant}/schedule/2020/01`, getHeaders(token))

    expect(resp.status).toEqual(204)
  })

  it.skip('validate valid schedule', async () => {
    jest.setTimeout(20000)
    const scheduleResp = await fetch(`${backendBaseUrl}/api/${tenant}/schedule/2023/01`, getHeaders(token))

    const validSchedule = await scheduleResp.json()

    const scheduleForValidation = {
      shifts: validSchedule.shifts.map(s => ({
        day: s.day,
        workplace: s.workplace,
        userId: s.user.userId
      }))
    }

    const validationResp = await fetch(`${backendBaseUrl}/api/${tenant}/schedule/2023/01/:validate`, {
      ...getHeaders(token),
      method: 'put',
      body: JSON.stringify(scheduleForValidation)
    })

    expect(validationResp.status).toEqual(200)
    const validationResult = await validationResp.json()
    expect(validationResult).toEqual(expect.objectContaining({ isValid: true }))
    expect(!validationResult.validationErrors || validationResult.validationErrors.length == 0).toBeTruthy()
  })

  it('return validation errors given invalid schedule', async () => {
    jest.setTimeout(20000)
    const scheduleResp = await fetch(`${backendBaseUrl}/api/${tenant}/schedule/2023/01`, getHeaders(token))

    const validSchedule = await scheduleResp.json()

    const scheduleForValidation = {
      shifts: validSchedule.shifts.map(s => ({
        day: s.day,
        workplace: s.workplace,
        userId: validSchedule.shifts[0].user.userId
      }))
    }

    fetch(`${backendBaseUrl}/api/${tenant}/schedule/2023/01/:validate`, {
      ...getHeaders(token),
      method: 'put',
      body: JSON.stringify(scheduleForValidation),

    })
      .then(x => console.log(x))
      .catch(reason => {
      console.log(reason)
    })

    const validationResp = await fetch(`${backendBaseUrl}/api/${tenant}/schedule/2023/01/:validate`, {
    ...getHeaders(token),
      method: 'put',
      body: JSON.stringify(scheduleForValidation)
    })

    expect(validationResp.status).toEqual(200)
    const validationResult = await validationResp.json()
    expect(validationResult).toEqual(expect.objectContaining({ isValid: false, validationErrors: expect.toBeArray() }))
    expect(validationResult.validationErrors).not.toBeEmpty()
  })
})
