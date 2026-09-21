import type { EducationalInstitute } from '../types/educationalInstitutes'

export async function getEducationalInstitutes(): Promise<
  EducationalInstitute[]
> {
  const response = await fetch('/api/educational-institutes')

  if (!response.ok) {
    throw new Error('Failed to fetch educational institutes')
  }

  return response.json()
}
