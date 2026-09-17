import { createFileRoute } from '@tanstack/react-router'
import { PageContainer, AuthCard } from '../components/layout'

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
