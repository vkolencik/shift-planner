import { compareMonths, Month } from './dates'

const baseWorkplaces = [
  'A1',
  'A2',
  'A3',
  'RES',
  'KPR'
]

export const getWorkplaces = (month: Month) => compareMonths(month, {year: 2023, month: 11}) <= 0 ? [ ...baseWorkplaces, 'URG' ] : baseWorkplaces
