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
