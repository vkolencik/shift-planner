import type {Config} from 'jest'

const config: Config = {
  preset: 'ts-jest',
  testEnvironment: 'node',
  silent: false,
  globals: {
    backendBaseUrl: 'https://b.krtek.me',
    // backendBaseUrl: 'http://localhost:65041',
    testUserId: 'auth0|633015ee115794a47e8166d8',

    'ts-jest': {
      tsconfig: 'tsconfig.json'
    }
  }
};

export default config
