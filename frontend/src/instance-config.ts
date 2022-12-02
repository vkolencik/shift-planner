function getWindowLocation (): Location {
  if (window == null || window.location == null) {
    throw new Error('Unable to determine location. Not running in browser?')
  }

  return window.location
}

export enum Instance {
  Production = 'prod',
  Preview = 'prev',
  Dev = 'dev'
}

export function getBaseUrl (): string {

  const urlMap = {
    [Instance.Dev]: 'http://localhost:65041',
    [Instance.Preview]: 'https://b.preview.krtek.me',
    [Instance.Production]: 'https://b.krtek.me',

  }

  const instance = getInstance()

  return urlMap[instance]
}

export function getInstance (): Instance {
  const hostname = getWindowLocation().hostname

  if (hostname === 'localhost') {
    return Instance.Dev
  }

  if (hostname.startsWith('pr-') && hostname.endsWith('amplifyapp.com')) {
    return Instance.Preview
  }

  if (hostname.endsWith('krtek.me')) {
    return Instance.Production
  }

  throw new Error(`Unknown environment with hostname ${hostname}`)

}
