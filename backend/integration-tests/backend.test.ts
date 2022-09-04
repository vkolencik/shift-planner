declare var backendBaseUrl: string

describe('backend should', () => {
  it('return unauthorized when auth token is not present in the request', async () => {
    const resp = await fetch(`${backendBaseUrl}/api/values`)
    expect(resp.status).toEqual(401)
  })

  it('return values', async () => {
    const token = '' // TODO
    const resp = await fetch(`${backendBaseUrl}/api/values`, {
      headers: {
        Authorization: `Bearer ${token}`,
      }
    })

    expect(resp.status).toEqual(200)
    expect(await resp.json()).toEqual(["value1", "value2"])
  })
})
