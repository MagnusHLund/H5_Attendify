import { createFileRoute } from '@tanstack/react-router'
import { PageContainer } from '../../components/layout'
import { OverviewPage } from '../../features/overview/OverviewPage'
export const Route = createFileRoute('/_authenticated/overview')({
  staticData: {
    pageNameKey: 'navigation.overview',
  },
  component: RouteComponent,
})

function RouteComponent() {
  return (
    <PageContainer>
      <OverviewPage />
    </PageContainer>
  )
}
