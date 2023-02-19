import { getActiveMonth } from './dates'

describe('dates should', () => {
  beforeAll(() => {
    jest.useFakeTimers()
  })

  ;[
    { year: 2020, month: 3, date: 1, hours: 0, minutes: 0, expectedMonth: { year: 2020, month: 4 } },
    { year: 2023, month: 2, date: 28, hours: 23, minutes: 59, expectedMonth: { year: 2023, month: 4 } },
    { year: 2022, month: 12, date: 31, hours: 23, minutes: 59, expectedMonth: { year: 2023, month: 2 } },
    { year: 2023, month: 5, date: 24, hours: 23, minutes: 59, expectedMonth: { year: 2023, month: 6 } },
    { year: 2023, month: 5, date: 25, hours: 0, minutes: 0, expectedMonth: { year: 2023, month: 7 } },
  ].forEach(({ year, month, date, hours, minutes, expectedMonth }) =>
    it(`get current month for ${year}-${month}-${date} ${hours}:${minutes}`, () => {
      jest.setSystemTime(new Date(year, month - 1, date, hours, minutes))

      const currentMonth = getActiveMonth()

      expect(currentMonth).toEqual(expectedMonth)
    }))
})
