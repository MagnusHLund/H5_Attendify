interface RegisterStudentOptions {
  email: string
  password: string
  educationalInstituteId: string
  studentId: string
  straightPhoto: File
  leftPhoto: File
  rightPhoto: File
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
  const formData = new FormData()
  formData.append('email', email)
  formData.append('password', password)
  formData.append('educationalInstituteId', educationalInstituteId)
  formData.append('studentId', studentId)
  formData.append('straightPhoto', straightPhoto)
  formData.append('leftPhoto', leftPhoto)
  formData.append('rightPhoto', rightPhoto)

  const response = await fetch('/api/auth/register', {
    method: 'POST',
    body: formData,
  })

  if (!response.ok) {
    throw new Error('Failed to register student')
  }
}
