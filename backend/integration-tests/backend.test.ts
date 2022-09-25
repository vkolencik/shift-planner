import { SecretsManager } from 'aws-sdk'

declare var backendBaseUrl: string

// Code adapted from here: https://docs.aws.amazon.com/code-samples/latest/catalog/javascript-secrets-secrets_getsecretvalue.js.html
function getSecretValue<T> (secretArn): Promise<T> {
  const client = new SecretsManager({
    region: 'eu-central-1'
  })

  return new Promise((resolve, reject) => {
    client.getSecretValue({ SecretId: secretArn }, (err, data) => {
      if (err) {
        reject(err)
      } else {
        if ('SecretString' in data) {
          resolve(JSON.parse(data.SecretString))
        } else {
          throw new Error('Decoding binary secrets not supported.')
        }
      }
    })
  })
}

interface TestUserCredentials {
  username: string,
  password: string,
  client_id: string,
  client_secret: string
}

async function getTestUserCredentials (): Promise<TestUserCredentials> {
  const secretArn = 'arn:aws:secretsmanager:eu-central-1:720500131615:secret:test-user-credentials-oNMZpk'
  const credentials = await getSecretValue<TestUserCredentials>(secretArn)

  if (!credentials.username
    || !credentials.password
    || !credentials.client_id
    || !credentials.client_secret) {
    throw new Error('Invalid value of loaded client credentials')
  }

  return credentials
}

const getToken = (): Promise<string> =>
  getTestUserCredentials()
    .then(testUserCredentials =>
      fetch('https://krtek.eu.auth0.com/oauth/token', {
        method: 'POST',
        headers: { 'content-type': 'application/x-www-form-urlencoded' },
        body: new URLSearchParams({
          grant_type: 'password',
          audience: 'https://b.krtek.me',
          scope: '',
          ...testUserCredentials
        })
      }))
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
