export async function isAuthenticated(): Promise<boolean> {
  // TODO: We need an endpoint to verify authentication status
  try {
    const response = await fetch('/api/auth/me', {
      credentials: 'include',
    })

    return response.ok
  } catch {
    return false
  }
}
