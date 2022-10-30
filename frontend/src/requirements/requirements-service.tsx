// const baseUrl = 'https://b.krtek.me'
import { useAuth0 } from '@auth0/auth0-react'
import { getBaseUrl } from '../get-base-url'

const baseUrl = getBaseUrl()
const tenant = 'fnb'

export const requirementTypes: { [key in RequirementType]: { name: string, value: RequirementType } } = {
  'leave': { name: 'Dovolená', value: 'leave' },
  'soft-yes': { name: 'Chci', value: 'soft-yes' },
  'hard-no': { name: 'Nemůžu', value: 'hard-no' },
  'only-morning': { name: 'Pouze R', value: 'only-morning' },
  'only-afternoon': { name: 'Až odpoledne', value: 'only-afternoon' }
}

export type RequirementType = 'leave' | 'hard-no' | 'soft-yes' | 'only-morning' | 'only-afternoon'

export type EmployeeMonthRequirements = { [key: string]: RequirementType }
const requirements: EmployeeMonthRequirements = {
  '2022-12-06': 'soft-yes',
  '2022-12-08': 'soft-yes',
  '2022-12-21': 'leave',
  '2022-12-13': 'hard-no'
}

export function addRequirement (user: any, date: string, type: RequirementType) {
  requirements[date] = type
}

export function removeRequirement (user: any, date: string) {
  delete requirements[date]
}

export function useApi () {
  const { user, getAccessTokenSilently, isAuthenticated } = useAuth0()

  if (!isAuthenticated || user === undefined || user.sub === undefined) {
    throw new Error('User is not logged in')
  }

  const userId = user.sub

  return ({
    requirements: {
      get: async (year: number, month: number): Promise<EmployeeMonthRequirements> =>
        getAccessTokenSilently({
          audience: 'https://b.krtek.me',
          scope: '',
        })
          .then(token => fetch(
            `${baseUrl}/api/${tenant}/users/${encodeURIComponent(userId)}/requirements/${year}/${month}`,
            {
              headers: {
                Authorization: `Bearer ${token}`
              }
            }
          ))
          .then(resp => {
            if (resp.status !== 200) {
              throw new Error('')
            }

            return resp.json()
          })
    }
  })
}
