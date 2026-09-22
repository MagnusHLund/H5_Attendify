import { fetchApi } from '../../../lib/api/client'
import type { EducationalInstitute } from '../types/educationalInstitutes'

export async function getEducationalInstitutes(): Promise<
  EducationalInstitute[]
> {
  const response = await fetchApi('/api/educational-institutes')

  if (!response.ok) {
    throw new Error('Failed to fetch educational institutes')
  }

  return response.json()
}
