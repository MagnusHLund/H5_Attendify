import { createFileRoute } from '@tanstack/react-router'
import { PageContainer } from '../../components/layout'
import { SettingsPage } from '../../features/settings/SettingsPage'
export const Route = createFileRoute('/_authenticated/settings')({
  staticData: {
    pageName: 'Settings',
  },
  component: RouteComponent,
})

function RouteComponent() {
  // TODO: If user is administrator, redirect to overview

  return (
    <PageContainer>
      <SettingsPage />
    </PageContainer>
  )
}
