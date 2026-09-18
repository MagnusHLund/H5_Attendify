import { createFileRoute } from '@tanstack/react-router'
import { PageContainer } from '../../components/layout'
export const Route = createFileRoute('/_authenticated/overview')({
  staticData: {
    pageName: 'Overview',
  },
  component: RouteComponent,
})

function RouteComponent() {
  return <PageContainer>Hello "/overview"!</PageContainer>
}
