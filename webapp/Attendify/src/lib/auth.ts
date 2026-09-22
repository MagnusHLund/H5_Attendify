import { fetchApi } from './api/client'

export async function isAuthenticated(): Promise<boolean> {
  try {
    const response = await fetchApi('/api/auth/me', {
      credentials: 'include',
    })

    return response.ok
  } catch {
    return false
  }
}
