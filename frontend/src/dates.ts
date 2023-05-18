import moment, { Moment } from 'moment'

export function getMonthLabel (month: Month) {
  return getStartOfMonth(month).toLocaleString('default', { month: 'long' }) + ' ' + month.year
}

export const getStartOfMonth = (month: Month): Date => new Date(month.year, month.month - 1, 1)

export const formatDate = (date: Day) => {
  const [year, month, day] = date.split('-').map(dateComponent => parseInt(dateComponent, 10))

  return `${day}. ${month}. ${year}`
}

export const formatDateMoment = (date: Moment) => date.format('dd. MM. yyyy')

export type Day = string & { __isDay: true }

export const parseDate = (date: string): Date => {
  const [year, month, day] = date.split('-').map(dateComponent => parseInt(dateComponent, 10))

  return new Date(year, month - 1, day)
}

export function getDayOfWeekNumber(day: Day): number {
  const date = parseDate(day)

  const dayOfWeek = date.getDay()

  return dayOfWeek === 0 ? 7 : dayOfWeek
}

export function isWeekend (day: Day) {
  const dayOfWeek = getDayOfWeekNumber(day)
  return dayOfWeek === 6 || dayOfWeek === 7
}

export function toDay (date: string): Day {
  return date as Day
}

const dayFormat = 'YYYY-MM-DD'

function getMoment (date: Day): Moment {
  return moment(date, dayFormat)
}

function formatMoment (moment: Moment): Day {
  return toDay(moment.format(dayFormat))
}

export function addDays (date: Day, days: number): Day {
  return formatMoment(getMoment(date).add(days, 'days'))
}

export function addMonths (month: Month, months: number) {
  return getMonth(formatMoment(moment(getStartOfMonth(month)).add(months, 'months')))
}

export function getNumberOfDaysOfMonth (month: Month) {
  return new Date(month.year, month.month, 0).getDate()
}

export interface Month {
  year: number,
  month: number
}

export function getActiveMonth (): Month {
  const today = moment()

  if (today.year() === 2023 && today.month() === 2 && today.date() >= 17) {
    return { year: 2023, month: 5 }
  }

  const monthsToAdd = today.date() < 25 ? 1 : 2
  const currentMonth = today.add(monthsToAdd, 'month')

  return { year: currentMonth.year(), month: currentMonth.month() + 1 }
}

export function isMonthWritable (month: Month): boolean {
  const today = moment()

  if (today.year() === 2023 && today.month() === 2 && today.date() > 5) {
    return (17 <= today.date() && today.date() <= 26)
  }

  if (today.year() === 2023 && today.month() === 3 && today.date() < 25) {
    return false
  }

  return today.date() >= 25 || today.date() <= 5
}

export function getMonthDays (month: Month): number[] {
  return new Array(getNumberOfDaysOfMonth(month)).fill(1).map((_, i) => i + 1)
}

export function getMonthDates (month: Month): Day[] {
  return getMonthDays(month).map(d => getDay(month.year, month.month, d))
}

export function compareMonths(month1: Month, month2: Month): (-1 | 0 | 1) {
  const date1 =  getStartOfMonth(month1).getTime()
  const date2 =  getStartOfMonth(month2).getTime()

  if (date1 == date2) {
    return 0
  }

  return date1 < date2 ? -1 : 1
}

export function getDay (year: number, month: number, day: number): Day {
  return toDay(`${year}-${padDateComponent(month)}-${padDateComponent(day)}`)
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

export function getStartOfWeek (): Day {
  return formatMoment(moment().startOf('isoWeek'))
}

export function getWeekDays (startOfWeek: Day): Day[] {
  const days = []
  for (let d = 0; d < 7; d++) {
    days.push(addDays(startOfWeek, d))
  }

  return days
}

export function getMonth (date: Day): Month {
  const moment = getMoment(date)
  return {
    year: moment.year(),
    month: moment.month() + 1
  }
}

export function getMonthDay (date: Day): number {
  const moment = getMoment(date)
  return moment.date()
}
