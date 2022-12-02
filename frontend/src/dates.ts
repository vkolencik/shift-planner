export const currentMonth = 1
export const currentYear = 2023

export function getCurrentMonthString () {
  return getStartOfCurrentMonth().toLocaleString('default', { month: 'long' }) + ' ' + currentYear
}

export const getStartOfCurrentMonth = () => new Date(currentYear, currentMonth - 1, 1)

export const formatDate = (date: string) => {
  const [year, month, day] = date.split('-').map(dateComponent => parseInt(dateComponent, 10))

  return `${day}. ${month}. ${year}`
}
export type Day = string & { __isDay: true }

export const parseDate = (date: string) => {
  const [year, month, day] = date.split('-').map(dateComponent => parseInt(dateComponent, 10))

  return new Date(year, month - 1, day)
}

export const isWeekend = (date: Date) => date.getDay() == 0 || date.getDay() == 6

export function toDay (date: string): Day {
  return date as Day
}

export function getNumberOfDaysOfCurrentMonth() {
  return new Date(currentYear, currentMonth, 0).getDate()
}
