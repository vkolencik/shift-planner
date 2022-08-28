import type {Config} from 'jest'

const config: Config = {
  preset: 'ts-jest',
  testEnvironment: 'node',
  globals: {
    // backendBaseUrl: 'https://9h0nneeao3.execute-api.eu-central-1.amazonaws.com/prod'
    backendBaseUrl: 'https://b.krtek.me/prod'
  }
};

export default config
