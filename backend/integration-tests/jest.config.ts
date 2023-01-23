import type {Config} from 'jest'

const config: Config = {
  preset: 'ts-jest',
  testEnvironment: 'node',
  silent: false,
  setupFilesAfterEnv: ['jest-extended/all'],
  testTimeout: 120000,
  globals: {
    backendBaseUrl: 'http://localhost:65041',
    testUserId: 'auth0|633015ee115794a47e8166d8',
    testUserName: 'Test User 1',
    testUser2Id: 'auth0|6367b6f40df1e7bc1037834f',
    testUser2Name: 'Test User 2',
    'ts-jest': {
      tsconfig: 'tsconfig.json'
    }
  }
}

export default config
