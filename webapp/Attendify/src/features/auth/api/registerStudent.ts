import { fetchApi } from '../../../lib/api/client'

interface RegisterStudentOptions {
  email: string
  password: string
  educationalInstituteId: string
  studentId: string
  straightPhoto: string
  leftPhoto: string
  rightPhoto: string
}

export async function registerStudent({
  email,
  password,
  educationalInstituteId,
  studentId,
  straightPhoto,
  leftPhoto,
  rightPhoto,
}: RegisterStudentOptions): Promise<void> {
  const payload = {
    email,
    password,
    educationalInstituteId,
    studentId,
    straightPhoto,
    leftPhoto,
    rightPhoto,
  }

  const response = await fetchApi('/api/auth/register', {
    method: 'POST',
    body: JSON.stringify(payload),
    headers: {
      'Content-Type': 'application/json',
    },
  })

  if (!response.ok) {
    throw new Error('Failed to register student')
  }
}
