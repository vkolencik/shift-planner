declare var backendBaseUrl: string

describe('test', () => {
  it('test2', async () => {
    const resp = await fetch(`${backendBaseUrl}/api/values`)
    expect(resp.status).toEqual(200)
    expect(await resp.json()).toEqual(["value1", "value2"])
  })
})
