import type {Config} from 'jest'

const config: Config = {
  preset: 'ts-jest',
  testEnvironment: 'node',
  silent: false,
  setupFilesAfterEnv: ['jest-extended/all'],
  globals: {
    backendBaseUrl: 'http://localhost:65041',
    testUserId: 'auth0|633015ee115794a47e8166d8',

    'ts-jest': {
      tsconfig: 'tsconfig.json'
    }
  }
};

export default config
