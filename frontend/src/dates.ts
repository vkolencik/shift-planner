export const currentMonth = 0
export const currentYear = 2023

export function getCurrentMonthString () {
  return new Date(currentYear, currentMonth, 1).toLocaleString('default', { month: 'long' }) + ' ' + currentYear
}

export const formatDate = (date: string) => {
  const [year, month, day] = date.split('-').map(dateComponent => parseInt(dateComponent, 10))

  return `${day}. ${month}. ${year}`
}
