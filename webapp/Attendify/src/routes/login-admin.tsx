import { createFileRoute } from '@tanstack/react-router'
import { PageContainer } from '../components/layout'
import { AuthCard, LoginAdminForm } from '../features/auth/components'

export const Route = createFileRoute('/login-admin')({
  component: RouteComponent,
})

function RouteComponent() {
  return (
    <PageContainer>
      <AuthCard>
        <LoginAdminForm />
      </AuthCard>
    </PageContainer>
  )
}
