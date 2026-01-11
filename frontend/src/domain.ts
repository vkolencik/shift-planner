import { compareMonths, Month } from './dates'

const baseWorkplaces = [
  'A1',
  'A2',
  'RES',
  'KPR'
]

export const getWorkplaces = (month: Month) => compareMonths(month, {year: 2023, month: 10}) <= 0 ? [ ...baseWorkplaces, 'URG', 'A3' ] : baseWorkplaces
