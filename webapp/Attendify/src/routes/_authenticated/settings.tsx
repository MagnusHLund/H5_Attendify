import { createFileRoute } from '@tanstack/react-router'

export const Route = createFileRoute('/_authenticated/settings')({
  staticData: {
    pageName: 'Settings',
  },
  component: RouteComponent,
})

function RouteComponent() {
  return <div>Hello "/settings"!</div>
}
