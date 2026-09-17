import { createFileRoute, redirect } from '@tanstack/react-router'
import { isAuthenticated } from '../lib/auth'
import { PageContainer, AuthCard } from '../components/layout'

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
    <PageContainer>
      <AuthCard>
        <h1>Login</h1>
      </AuthCard>
    </PageContainer>
  )
}
