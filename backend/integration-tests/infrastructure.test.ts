/// <reference types="jest-extended" />

describe('backend should', () => {
  it('return OK on live endpoint', async () => {
    const resp = await fetch(backendBaseUrl)
    expect(resp.status).toEqual(200)
  })

  describe('CORS', () => {
    ['http://localhost:3000', 'https://krtek.me', 'https://www.krtek.me']
      .forEach(origin=>
        it(`allow origin ${origin}`, async () => {
          const response = await fetch(backendBaseUrl, {
            headers: {
              Origin: origin
            }
          })

          expect(response.headers.get('Access-Control-Allow-Origin')).toBeOneOf(['*', origin])
        })
      )

    it('not allow other origins', async () => {
      const origin = 'https://phishers-price.biz'
      const response = await fetch(backendBaseUrl, {
        headers: {
          Origin: origin
        }
      })

      expect(response.headers.get('Access-Control-Allow-Origin')).toBeFalsy()
    })
  })
})
