import {
  compareMonths, Day,
  getActiveMonth,
  getDayOfWeekNumber,
  isActiveMonthWritable,
  isWeekend,
  Month,
  toDay
} from './dates'

describe('dates should', () => {
  beforeAll(() => {
    jest.useFakeTimers()
  })

  describe(getActiveMonth.name, () => {
    ;[
      { year: 2020, month: 3, date: 1, hours: 0, minutes: 0, expectedMonth: { year: 2020, month: 4 } },
      { year: 2023, month: 2, date: 28, hours: 23, minutes: 59, expectedMonth: { year: 2023, month: 4 } },
      { year: 2022, month: 12, date: 31, hours: 23, minutes: 59, expectedMonth: { year: 2023, month: 2 } },
      { year: 2023, month: 5, date: 24, hours: 23, minutes: 59, expectedMonth: { year: 2023, month: 6 } },
      { year: 2023, month: 5, date: 25, hours: 0, minutes: 0, expectedMonth: { year: 2023, month: 7 } },
      { year: 2023, month: 3, date: 17, hours: 0, minutes: 0, expectedMonth: { year: 2023, month: 5 } }
    ].forEach(({ year, month, date, hours, minutes, expectedMonth }) =>
      it(`get current month for ${year}-${month}-${date} ${hours}:${minutes}`, () => {
        jest.setSystemTime(new Date(year, month - 1, date, hours, minutes))

        const currentMonth = getActiveMonth()

        expect(currentMonth).toEqual(expectedMonth)
      }))
  })

  describe(isWeekend.name, () => {
    ;[
      '2023-02-25', '2023-02-26'
    ].map(d => toDay(d)).forEach(d => it(`Consider ${d} to be weekend day`, () => {
      expect(isWeekend(d)).toEqual(true)
    }))

  })

  describe(getDayOfWeekNumber.name, () => {
    ([
      ['2023-02-20', 1],
      ['2023-02-21', 2],
      ['2023-02-22', 3],
      ['2023-02-23', 4],
      ['2023-02-24', 5],
      ['2023-02-25', 6],
      ['2023-02-26', 7]
    ] as [string, number][]).forEach(([day, expectedWeekdayNumber]) => it(`Return weekday number for ${day}`, () => {
      expect(getDayOfWeekNumber(toDay(day))).toEqual(expectedWeekdayNumber)
    }))
  })

  describe(compareMonths.name, () => {
    ([
      [{ year: 2023, month: 1 }, { year: 2023, month: 2 }, -1],
      [{ year: 2023, month: 1 }, { year: 2023, month: 1 }, 0],
      [{ year: 2023, month: 1 }, { year: 2022, month: 12 }, 1],
      [{ year: 2022, month: 5 }, { year: 2022, month: 3 }, 1],
    ] as [Month, Month, number][]).forEach(([month1, month2, expectedResult]) =>
      it(`Compare months ${JSON.stringify(month1)} and ${JSON.stringify(month2)}`, () => {
        expect(compareMonths(month1, month2)).toEqual(expectedResult)
      }))
  })

  describe(isActiveMonthWritable.name, () => {
    ([
      { month: 2, date: 24, shouldBeWritable: false },
      { month: 2, date: 25, shouldBeWritable: true },
      { month: 3, date: 4, shouldBeWritable: true },
      { month: 3, date: 5, shouldBeWritable: true },
      { month: 3, date: 6, shouldBeWritable: false },
      { month: 3, date: 17, shouldBeWritable: true },
      { month: 3, date: 25, shouldBeWritable: true },
      { month: 3, date: 26, shouldBeWritable: false }
    ]).forEach(data => it(`${data.shouldBeWritable ? 'should' : 'shouldn\'t'} be writable on 2023-${data.month}-${data.date}`, () => {
      jest.setSystemTime(new Date(2023, data.month - 1, data.date, 0, 1))

      const isWritable = isActiveMonthWritable()

      expect(isWritable).toEqual(data.shouldBeWritable)
    }))
  })
})
