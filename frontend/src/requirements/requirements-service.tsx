// const baseUrl = 'https://b.krtek.me'
import { useAuth0 } from '@auth0/auth0-react'
import { getBaseUrl } from '../get-base-url'
import { Moment } from 'moment'

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

export type EmployeeMonthRequirements = { [key: Day]: RequirementType }

export type Day = string & { __isDay: true }

export function toDay (date: string): Day { return date as Day }

export function momentToDay (date: Moment): Day { return toDay(date.format('yyyy-MM-DD'))}

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
          }),
      put: async (date: Day, requirementType: RequirementType): Promise<void> => {
        const token = await getAccessTokenSilently({
          audience: 'https://b.krtek.me',
          scope: ''
        })

        if (!date.match(/^\d{4}-\d{2}-\d{2}$/)) {
          throw new Error(`Invalid date format ${date}`)
        }

        const response = await fetch(`${baseUrl}/api/${tenant}/users/${encodeURIComponent(userId)}/requirements/${date.replace(/-/g, '/')}`, {
            method: 'PUT',
            body: JSON.stringify({ requirementType: requirementType }),
            headers: {
              Authorization: `Bearer ${token}`,
              'Content-Type': 'application/json',
            }
          }
        )

        if (response.status !== 200) {
          throw new Error(`Error saving requirement: ${await response.text()}`)
        }
      },
      delete: async (date: Day): Promise<void> => {
        throw new Error('Deletion not implemented yet')
      }
    }
  })
}
