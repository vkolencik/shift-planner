import { getBaseUrl, getInstance, Instance } from './instance-config'

describe('Base URL should', () => {
  [
    {
      frontendUrl: 'http://localhost:3000',
      expectedBackendUrl: 'http://localhost:65041'
    },
    {
      frontendUrl: 'https://krtek.me/',
      expectedBackendUrl: 'https://b.krtek.me'
    },
    {
      frontendUrl: 'https://www.krtek.me/',
      expectedBackendUrl: 'https://b.krtek.me'
    },
    {
      frontendUrl: 'https://pr-56.d13dc8ck6htkvt.amplifyapp.com/',
      expectedBackendUrl: 'https://b.preview.krtek.me'
    }
  ].forEach(({frontendUrl, expectedBackendUrl}) =>
    it(`returns correct environment name for "${frontendUrl}"`, () => {
      // @ts-expect-error
      delete window.location
      // @ts-expect-error
      window.location = new URL(frontendUrl)

      const backendUrl = getBaseUrl()

      expect(backendUrl).toEqual(expectedBackendUrl)
    }))

  ;[
    {
      frontendUrl: 'http://localhost:3000',
      expectedInstance: Instance.Dev
    },
    {
      frontendUrl: 'https://krtek.me/',
      expectedInstance: Instance.Production
    },
    {
      frontendUrl: 'https://www.krtek.me/',
      expectedInstance: Instance.Production
    },
    {
      frontendUrl: 'https://pr-56.d13dc8ck6htkvt.amplifyapp.com/',
      expectedInstance: Instance.Preview
    }
  ].forEach(({frontendUrl, expectedInstance}) =>
    it(`returns correct environment name for "${frontendUrl}"`, () => {
      // @ts-expect-error
      delete window.location
      // @ts-expect-error
      window.location = new URL(frontendUrl)

      const instance = getInstance()

      expect(instance).toEqual(expectedInstance )
    }))
})

export {}
