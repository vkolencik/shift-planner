import type {Config} from 'jest'
import baseConfig from './jest.config'

const config: Config = {
  ...baseConfig,
  globals: {
    ...baseConfig.globals,
    backendBaseUrl: 'https://b.krtek.me'
  },
  testTimeout: 15000
};

export default config
