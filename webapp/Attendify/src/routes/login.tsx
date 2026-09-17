import { createFileRoute, redirect } from '@tanstack/react-router'
import { isAuthenticated } from '../lib/auth'
import { PageContainer } from '../components/layout'
import { AuthCard, LoginForm } from '../features/auth/components'

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
        <LoginForm />
      </AuthCard>
    </PageContainer>
  )
}
