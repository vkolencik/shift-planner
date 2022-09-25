import type {Config} from 'jest'

const config: Config = {
  preset: 'ts-jest',
  testEnvironment: 'node',
  globals: {
    backendBaseUrl: 'https://b.krtek.me'
    // backendBaseUrl: 'http://localhost:65041'
  }
};

export default config
