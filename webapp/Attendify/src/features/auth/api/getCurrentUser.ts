import { fetchApi } from '../../../lib/api/client';
import type { CurrentUser } from '../types/user'

export async function getCurrentUser(): Promise<CurrentUser> {
  const response = await fetchApi('/api/auth/me', {
    credentials: 'include',
  })

  if (!response.ok) {
    throw new Error('Failed to fetch current user')
  }

  return response.json()
}
