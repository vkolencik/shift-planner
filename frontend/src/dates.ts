import moment, { Moment } from 'moment'

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
export const formatDateMoment = (date: Moment) => date.format('dd. MM. yyyy')

export type Day = string & { __isDay: true }

export const parseDate = (date: string) => {
  const [year, month, day] = date.split('-').map(dateComponent => parseInt(dateComponent, 10))

  return new Date(year, month - 1, day)
}

export function isWeekend (day: string) {
  const date = parseDate(day)
  return date.getDay() == 0 || date.getDay() == 6
}

export function toDay (date: string): Day {
  return date as Day
}

export function getNumberOfDaysOfMonth (month: Month) {
  return new Date(month.year, month.month, 0).getDate()
}

export interface Month {
  year: number,
  month: number
}

export function getCurrentMonth (): Month {
  return { year: currentYear, month: currentMonth }
}

export function getMonthDays (month: Month): number[] {
  return new Array(getNumberOfDaysOfMonth(month)).fill(1).map((_, i) => i + 1)
}

export function getMonthDates (month: Month): string[] {
  return getMonthDays(month).map(d => dateToString(month.year, month.month, d))
}

export function dateToString (year: number, month: number, day: number) {
  return `${year}-${padDateComponent(month)}-${padDateComponent(day)}`
}

function padDateComponent (value: number): string {
  return String(value).padStart(2, '0')
}

export function getDayOfWeek (day: string): string {
  return [
    'Ne',
    'Po',
    'Út',
    'St',
    'Čt',
    'Pá',
    'So',
  ][parseDate(day).getDay()]
}

export function getStartOfWeek (): Moment {
  return moment().startOf('isoWeek')
}

export function getWeekDays (startOfWeek: Moment): Moment[] {
  const days = []
  for (let d = 0; d < 7; d++) {
    days.push(startOfWeek.clone().add(d, 'days'))
  }

  return days
}

export function getMonth (date: Moment): Month {
  return {
    year: date.year(),
    month: date.month() + 1
  }
}
