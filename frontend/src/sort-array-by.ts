export function sortArrayBy<T> (array: T[], func: (val: T) => string) {
  const clt = Intl.Collator('cs')
  array.sort((a, b) => clt.compare(func(a), func(b)))
}
