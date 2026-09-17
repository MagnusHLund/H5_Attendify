import { createFileRoute } from '@tanstack/react-router'
import { PageContainer } from '../components/layout'
import { AuthCard, ResetPasswordForm } from '../features/auth/components'

export const Route = createFileRoute('/reset-password')({
  component: RouteComponent,
})

function RouteComponent() {
  return (
    <PageContainer>
      <AuthCard>
        <ResetPasswordForm />
      </AuthCard>
    </PageContainer>
  )
}
