export function sortArrayBy<T> (array: T[], func: (val: T) => any) {
  array.sort((a, b) => func(a) > func(b) ? 1 : (func(a) < func(b) ? -1 : 0))
}
