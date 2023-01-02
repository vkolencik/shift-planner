export const wait = (timeoutMs: number): Promise<void> => new Promise((resolve) => {
  setTimeout(resolve, timeoutMs);
})
