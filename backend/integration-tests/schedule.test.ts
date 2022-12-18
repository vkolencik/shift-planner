import { getToken } from './helpers/auth'
import { getHeaders } from './helpers/getHeaders'

describe('schedule endpoint should', () => {
  const tenant = 'fnb'
  let token
  let token2
  beforeAll(async () => {
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
})
