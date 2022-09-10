declare var backendBaseUrl: string

const getToken = (): Promise<string> =>
  fetch('https://krtek.eu.auth0.com/oauth/token', {
    method: 'POST',
    headers: {'content-type': 'application/x-www-form-urlencoded'},
    body: new URLSearchParams({
      grant_type: 'password',
      username: 'test_username@example.com',
      password: 'test_user_password',
      audience: 'https://b.krtek.me',
      scope: '',
      client_id: '1Ui09NPmSmSCjGqropGY9VeLGup5vI2c',
      client_secret: '<client secret>'
    })
  })
    .then(resp => resp.json())
    .then(respBody => {
      if (respBody.error) {
        throw new Error(`${respBody.error} (${respBody.error_description})`)
      }
      return respBody.access_token
    })
    .catch(e => console.error(e))

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
    expect(await resp.json()).toEqual(['value1', 'value2'])
  })
})
