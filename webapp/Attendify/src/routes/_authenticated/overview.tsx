import { createFileRoute } from '@tanstack/react-router'

export const Route = createFileRoute('/_authenticated/overview')({
  staticData: {
    pageName: 'Overview',
  },
  component: RouteComponent,
})

function RouteComponent() {
  return <div>Hello "/overview"!</div>
}
