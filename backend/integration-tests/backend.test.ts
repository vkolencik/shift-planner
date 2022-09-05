declare var backendBaseUrl: string

const getToken = (): Promise<string> =>
  fetch ('https://krtek.eu.auth0.com/oauth/token', {
    method: 'POST',
    headers: { 'content-type': 'application/json' },
    body: '{}' // redacted
  })
  .then(resp => resp.json())
  .then(respBody => respBody.access_token)

describe('backend should', () => {
  it('return unauthorized when auth token is not present in the request', async () => {
    const resp = await fetch(`${backendBaseUrl}/api/values`)
    expect(resp.status).toEqual(401)
  })

  it('return values', async () => {
    const token: string = await getToken()
    const resp = await fetch(`${backendBaseUrl}/api/values`, {
      headers: {
        Authorization: `Bearer ${token}`,
      }
    })

    expect(resp.status).toEqual(200)
    expect(await resp.json()).toEqual(["value1", "value2"])
  })
})
