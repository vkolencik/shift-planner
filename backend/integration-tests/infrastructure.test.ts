describe('backend should', () => {
  it('return OK on live endpoint', async () => {
    const resp = await fetch(`${backendBaseUrl}`)
    expect(resp.status).toEqual(200)
  })
})
