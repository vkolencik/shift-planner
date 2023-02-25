import type {Config} from 'jest'

const config: Config = {
  preset: 'ts-jest',
  testEnvironment: 'node',
  silent: false,
//  setupFilesAfterEnv: ['jest-extended/all'],
  moduleNameMapper: {
    "\\.(css|less)$": "identity-obj-proxy"
  },
  transformIgnorePatterns: [
    '/node_modules/(?!(react[\\-]icons/all))'
  ],
  globals: {
  }
}

export default config
