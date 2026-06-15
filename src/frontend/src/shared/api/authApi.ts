import { httpClient } from './httpClient'

export type AuthenticateRequest = {
  username: string
  password: string
}

export type AuthenticateResponse = {
  authenticated: boolean
  authorizationHeader: string
  username: string
}

export async function authenticate(request: AuthenticateRequest) {
  const response = await httpClient.post<AuthenticateResponse>('/authenticate', request)
  return response.data
}
