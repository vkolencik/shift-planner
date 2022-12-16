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
    const schedule = responseData.schedule

    expect(schedule.length).toEqual(31)
    expect(schedule[0].date).toEqual('2023-01-01')
    expect(schedule[0].shifts.length).toEqual(6)
    expect(schedule[0].shifts.map(s => s.workplace)).toEqual(expect.arrayContaining([
      'A1','A2','A3','RES','KPR','URG'
    ]))

    expect(schedule[0].shifts.map(s => s.userId)).toSatisfyAll(userId => userId?.length > 0)
    expect(schedule[0].shifts.map(s => s.userName)).toSatisfyAll(userName => userName?.length > 0)
  })
})
