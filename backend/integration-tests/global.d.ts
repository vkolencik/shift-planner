/// <reference types="jest-extended" />
import 'jest-extended'

declare global {
  let backendBaseUrl: string
  let testUserId: string
  let testUserName: string
  let testUser2Id: string
  let testUser2Name: string
  let deactivatedUserId: string
  let deactivatedUserName: string
  let deactivatedUserActiveUntilDate: string
}

export {}
