// Code adapted from here: https://docs.aws.amazon.com/code-samples/latest/catalog/javascript-secrets-secrets_getsecretvalue.js.html
import {SecretsManager} from 'aws-sdk'

function getSecretValue<T>(secretArn): Promise<T> {
  const client = new SecretsManager({
    region: 'eu-central-1'
  })

  return new Promise((resolve, reject) => {
    client.getSecretValue({SecretId: secretArn}, (err, data) => {
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

export interface TestUserCredentials {
  username: string,
  password: string,
  client_id: string,
  client_secret: string
}

interface UserCredentials {
  username: string,
  password: string
}

interface StoredTestUsersCredentials {
  client_id: string,
  client_secret: string,
  testUser1: UserCredentials,
  testUser2: UserCredentials
}

type TestUser = 'testUser1' | 'testUser2'

async function getTestUserCredentials(testUser: TestUser): Promise<TestUserCredentials> {
  const secretArn = 'arn:aws:secretsmanager:eu-central-1:720500131615:secret:test-user-credentials-oNMZpk'
  const credentials = await getSecretValue<StoredTestUsersCredentials>(secretArn)

  if (!credentials
    || !credentials.client_id
    || !credentials.client_secret) {
    throw new Error('Invalid value of loaded client credentials')
  }

  return {
    username: credentials[testUser].username,
    password: credentials[testUser].password,
    client_id: credentials.client_id,
    client_secret: credentials.client_secret
  }
}

export const getToken = (testUser: TestUser = 'testUser1'): Promise<string> =>
  getTestUserCredentials(testUser)
    .then(testUserCredentials =>
      fetch('https://krtek.eu.auth0.com/oauth/token', {
        method: 'POST',
        headers: {'content-type': 'application/x-www-form-urlencoded'},
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
      const accessToken = respBody.access_token
      if (!accessToken) {
        throw new Error('Empty access token')
      }

      return accessToken
    })
