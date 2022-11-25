function getWindowLocation(): Location {
  if (window == null || window.location == null) {
    throw new Error('Unable to determine location. Not running in browser?')
  }

  return window.location
}

export function getBaseUrl(): string {
  const hostname = getWindowLocation().hostname

  if (hostname === 'localhost') {
    return 'http://localhost:65041'
  }

  if (hostname.startsWith('pr-') && hostname.endsWith('amplifyapp.com')) {
    return 'https://b.preview.krtek.me'
  }

  if (hostname.endsWith('krtek.me')) {
    return 'https://b.krtek.me'
  }

  throw new Error(`Unknown environment with hostname ${hostname}`)
}
