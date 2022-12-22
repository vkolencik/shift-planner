// const baseUrl = 'https://b.krtek.me'
import { useAuth0 } from '@auth0/auth0-react'
import { getBaseUrl, getInstance, Instance } from './instance-config'
import { Moment } from 'moment'
import { Day, Month, toDay } from './dates'
import { sortArrayBy } from './sort-array-by'

const baseUrl = getBaseUrl()
const tenant = 'fnb'

const inactiveUsers = [
  'auth0|637f84ec841043dd7a9d44d9', // Hladníková K.
  'auth0|637f84eb841043dd7a9d44d1', // Kršková
  'auth0|637f84ea841043dd7a9d44c9', // Matějková
  'auth0|637f84e6841043dd7a9d449d', // Švecová
]

const testUsers = [
  'auth0|637dde8af1e62a9cb6276431', // tester testovic
  'auth0|633015ee115794a47e8166d8', // test user 1
  'auth0|6367b6f40df1e7bc1037834f', // test user 2
  'auth0|6313a5ba9b10486b3d672dad', // lu
  'auth0|637dd38ca50041687c9806f9', // vojta
]

const excludedUserIds = getInstance() === Instance.Production ? [...inactiveUsers, ...testUsers] : inactiveUsers

function padDateComponent (value: number): string {
  return String(value).padStart(2, '0')
}

export const requirementTypes: { [key in RequirementType]: { name: string, value: RequirementType } } = {
  'leave': { name: 'Dovolená', value: 'leave' },
  'soft-yes': { name: 'Chci', value: 'soft-yes' },
  'hard-no': { name: 'Nemůžu', value: 'hard-no' },
  'only-morning': { name: 'Pouze R', value: 'only-morning' },
  'only-afternoon': { name: 'Až odpoledne', value: 'only-afternoon' }
}

export type RequirementType = 'leave' | 'hard-no' | 'soft-yes' | 'only-morning' | 'only-afternoon'

export interface UserRequirement {
  userId: string,
  userName: string,
  date: Day,
  requirementType: RequirementType,
  approved?: boolean | null
}

export type MonthRequirements = UserRequirement[]

export type EmployeeMonthRequirements = { [key: Day]: RequirementType }

export interface User { userId: string, userName: string }

export function momentToDay (date: Moment): Day {
  return toDay(date.format('yyyy-MM-DD'))
}

export interface Schedule {
  shifts: Shift[]
}

export interface Shift {
  day: Day,
  workplace: string,
  user: {userId: string, userName: string}
}

export function useApi () {
  const { user, getAccessTokenSilently, isAuthenticated } = useAuth0()

  if (!isAuthenticated || user === undefined || user.sub === undefined) {
    throw new Error('User is not logged in')
  }

  const loggedInUserId = user.sub

  const deleteForUser = async (userId: string, date: Day): Promise<void> => {
    const token = await getAccessTokenSilently({
      audience: 'https://b.krtek.me',
      scope: ''
    })

    if (!date.match(/^\d{4}-\d{2}-\d{2}$/)) {
      throw new Error(`Invalid date format ${date}`)
    }

    const response = await fetch(`${baseUrl}/api/${tenant}/users/${encodeURIComponent(userId)}/requirements/${date.replace(/-/g, '/')}`, {
        method: 'DELETE',
        headers: {
          Authorization: `Bearer ${token}`,
        }
      }
    )

    if (response.status !== 200) {
      throw new Error(`Error deleting requirement: ${await response.text()}`)
    }
  }

  return ({
    requirements: {
      get: async (month: Month): Promise<EmployeeMonthRequirements> =>
        getAccessTokenSilently({
          audience: 'https://b.krtek.me',
          scope: '',
        })
          .then(token => fetch(
            `${baseUrl}/api/${tenant}/users/${encodeURIComponent(loggedInUserId)}/requirements/${month.year}/${month.month}`,
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
      getMonth: async (month: Month): Promise<MonthRequirements> =>
        getAccessTokenSilently({
          audience: 'https://b.krtek.me',
          scope: '',
        })
          .then(token => fetch(
            `${baseUrl}/api/${tenant}/requirements/${month.year}/${padDateComponent(month.month)}`,
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

        const response = await fetch(`${baseUrl}/api/${tenant}/users/${encodeURIComponent(loggedInUserId)}/requirements/${date.replace(/-/g, '/')}`, {
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

      deleteForUser: deleteForUser,

      delete: async (date: Day): Promise<void> => deleteForUser(loggedInUserId, date),

      approve: async (userId: string, date: Day) => {
        const token = await getAccessTokenSilently({
          audience: 'https://b.krtek.me',
          scope: ''
        })

        if (!date.match(/^\d{4}-\d{2}-\d{2}$/)) {
          throw new Error(`Invalid date format ${date}`)
        }

        const response = await fetch(`${baseUrl}/api/${tenant}/users/${encodeURIComponent(userId)}/requirements/${date.replace(/-/g, '/')}/approval`, {
            method: 'PUT',
            body: JSON.stringify({ approved: true }),
            headers: {
              Authorization: `Bearer ${token}`,
              'Content-Type': 'application/json',
            }
          }
        )

        if (response.status !== 200) {
          throw new Error(`Error approving requirement: ${await response.text()}`)
        }
      }
    },

    users: {
      get: async (): Promise<User[]> => {
        const token = await getAccessTokenSilently({
          audience: 'https://b.krtek.me',
          scope: ''
        })

        const response = await fetch(`${baseUrl}/api/${tenant}/users`, {headers: { Authorization: `Bearer ${token}`}})

        if (response.status !== 200) {
          throw new Error(`Error fetching users: ${await response.text()}`)
        }

        const users = await response.json() as User[]
        sortArrayBy(users, u => u.userName)
        return users.filter(u => !excludedUserIds.includes(u.userId))
      }
    },

    schedule: {
      get: async (month: Month): Promise<Schedule | null> => {
        const token = await getAccessTokenSilently({
          audience: 'https://b.krtek.me',
          scope: ''
        })

        const response = await fetch(`${baseUrl}/api/${tenant}/schedule/${month.year}/${padDateComponent(month.month)}`, {headers: { Authorization: `Bearer ${token}`}})

        if (response.status >= 400) {
          throw new Error(`Error fetching users: ${await response.text()}`)
        }

        if (response.status === 204) {
          return null
        }

        const schedule = await response.json() as Schedule
        return {
          shifts: schedule.shifts.filter(s => !excludedUserIds.includes(s.user.userId))
        }
      }
    }
  })
}
