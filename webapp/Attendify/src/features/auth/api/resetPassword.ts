export async function requestResetPassword(email: string): Promise<void> {
  const payload = { email }

  const response = await fetch('/api/auth/password-reset/request', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  })

  if (!response.ok) {
    throw new Error('Failed to request password reset')
  }
}

export async function verifyResetPassword(securityCode: string): Promise<void> {
  const payload = { securityCode }

  const response = await fetch('/api/auth/password-reset/verify', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  })

  if (!response.ok) {
    throw new Error('Failed to verify password reset')
  }
}

export async function completeResetPassword(
  securityCode: string,
  newPassword: string,
): Promise<void> {
  const payload = { securityCode, newPassword }

  const response = await fetch('/api/auth/password-reset/complete', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  })

  if (!response.ok) {
    throw new Error('Failed to complete password reset')
  }
}
