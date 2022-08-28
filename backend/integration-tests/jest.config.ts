import type {Config} from 'jest'

const config: Config = {
  preset: 'ts-jest',
  testEnvironment: 'node',
  globals: {
    backendBaseUrl: 'https://b.krtek.me'
  }
};

export default config
