export async function loginWithStudentAccessCode(
  studentAccessCode: string,
): Promise<void> {
  const response = await fetch('/api/auth/login/access-code', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ studentAccessCode }),
  })

  if (!response.ok) {
    throw new Error('Failed to login using student access code')
  }
}
