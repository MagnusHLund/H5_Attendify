import type { StudentAccessCode } from '../types/studentAccessCode'

export async function getStudentAccessCode(): Promise<StudentAccessCode> {
  const response = await fetch('/api/settings/student-access-code', {
    credentials: 'include',
  })

  if (!response.ok) {
    throw new Error('Failed to fetch student access code')
  }

  return response.json()
}
