/// <reference types="jest-extended" />

describe('backend should', () => {
  it('return OK on live endpoint', async () => {
    const resp = await fetch(backendBaseUrl)
    expect(resp.status).toEqual(200)
  })

  describe('CORS', () => {
    [
      'http://localhost:3000',
      'https://krtek.me',
      'https://www.krtek.me',
      'https://pr-56.d13dc8ck6htkvt.amplifyapp.com'
    ]
      .forEach(origin =>
        it(`allow origin ${origin}`, async () => {
          const response = await fetch(backendBaseUrl, {
            headers: {
              Origin: origin
            }
          })

          const allowedOrigins = response.headers.get('Access-Control-Allow-Origin')
          expect(allowedOrigins).toBeOneOf(['*', origin])

          // const allowedHeaders = response.headers.get('Access-Control-Allow-Headers')
          // expect(allowedHeaders).toBeTruthy()
          // expect(allowedHeaders.split(/\w*,\w*/)).toContain('Authorization')
          //
          // const allowedMethods = response.headers.get('Access-Control-Allow-Methods')
          // expect(allowedMethods).toBeTruthy()
          // expect(allowedMethods.split(/\w*,\w*/)).toContainValues([
          //   'GET',
          //   'POST',
          //   'PUT',
          //   'DELETE'
          // ])
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
