import { createFileRoute, redirect } from '@tanstack/react-router'
import { isAuthenticated } from '../lib/auth'

export const Route = createFileRoute('/login')({
  beforeLoad: async () => {
    const authenticated = await isAuthenticated()

    if (authenticated) {
      throw redirect({
        to: '/overview',
      })
    }
  },

  component: LoginPage,
})

function LoginPage() {
  return (
    <div>
      <h1>Login</h1>
    </div>
  )
}
