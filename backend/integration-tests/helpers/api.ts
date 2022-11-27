import { getHeaders } from './getHeaders'

export type RequirementParams = { tenant: string, userId: string, year: number, month: number, day: number, requirementType: string }

function padDateComponent (value: number): string {
  return String(value).padStart(2, '0')
}

export async function putRequirement (params: RequirementParams, authToken: string): Promise<Response> {

  const resp = await fetch(
    `${backendBaseUrl}/api/${params.tenant}/users/${encodeURIComponent(params.userId)}/requirements/${params.year}/${padDateComponent(params.month)}/${padDateComponent(params.day)}`, {
      ...getHeaders(authToken),
      method: 'put',
      body: JSON.stringify({ requirementType: params.requirementType })
    })

  expect(resp.status).toEqual(200)

  return resp
}
