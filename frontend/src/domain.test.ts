import { getWorkplaces } from './domain'

describe('workplaces should', () => {
  ;[
    {year: 2023, month: 10},
    {year: 2023, month: 1},
    {year: 2022, month: 1},
  ].forEach(({year, month}) => it(`contain URG in ${year}-${month}`, () => {
    const workplaces = getWorkplaces({year, month})
    expect(workplaces).toContain('URG')
  }))

  ;[
    {year: 2023, month: 11},
    {year: 2024, month: 1},
    {year: 2024, month: 12},
  ].forEach(({year, month}) => it(`contain URG in ${year}-${month}`, () => {
    const workplaces = getWorkplaces({year, month})
    expect(workplaces).not.toContain('URG')
  }))
})
