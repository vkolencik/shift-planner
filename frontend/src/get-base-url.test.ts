import { getBaseUrl } from './get-base-url'

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
    }
  ].forEach(({ frontendUrl, expectedBackendUrl }) =>
    it(`returns correct environment name for "${frontendUrl}"`, () => {
      // @ts-expect-error
      delete window.location
      // @ts-expect-error
      window.location = new URL(frontendUrl)

      const backendUrl = getBaseUrl()

      expect(backendUrl).toEqual(expectedBackendUrl)
    }))
})

export {}
