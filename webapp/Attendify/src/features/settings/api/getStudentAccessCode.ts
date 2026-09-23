import { fetchApi } from '../../../lib/api/client'
import type { StudentAccessCode } from '../types/studentAccessCode'

export async function getStudentAccessCode(): Promise<StudentAccessCode> {
  const response = await fetchApi('/api/', {
    // TODO: Modify url path to match endpoint
    credentials: 'include',
  })

  if (!response.ok) {
    throw new Error('Failed to fetch student access code')
  }

  return response.json()
}
