export async function fetchApi(
  endpoint: string,
  options?: RequestInit,
): Promise<Response> {
  let response = await fetch(endpoint, options)

  if (response.status === 401) {
    const path = '/api/auth/refresh'
    const retryResponse = await fetch(path, { method: 'POST' })

    if (retryResponse.ok) {
      response = await fetch(endpoint, options)
    }
  }

  return response
}
