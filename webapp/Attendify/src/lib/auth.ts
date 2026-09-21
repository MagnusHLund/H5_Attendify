export async function isAuthenticated(): Promise<boolean> {
  try {
    const response = await fetch('/api/auth/me', {
      credentials: 'include',
    })

    return response.ok
  } catch {
    return false
  }
}
