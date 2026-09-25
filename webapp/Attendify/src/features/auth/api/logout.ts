import { withRefreshPaused } from '../../../lib/api/client'

export async function logout(): Promise<void> {
  await withRefreshPaused(async () => {
    const response = await fetch('/api/auth/logout', {
      method: 'POST',
      credentials: 'include',
      headers: {
        'Content-Type': 'application/json',
      },
    })

    if (!response.ok) {
      throw new Error('Failed to logout')
    }
  })
}
