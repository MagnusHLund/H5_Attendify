import { createFileRoute } from '@tanstack/react-router'
import { PageContainer } from '../components/layout'
import { AuthCard } from '../features/auth/components'

export const Route = createFileRoute('/register')({
  component: RegisterPage,
})

function RegisterPage() {
  return (
    <PageContainer>
      <AuthCard>Hello "/register"!</AuthCard>
    </PageContainer>
  )
}
