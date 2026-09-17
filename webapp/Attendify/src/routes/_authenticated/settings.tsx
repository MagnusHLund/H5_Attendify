import { createFileRoute } from '@tanstack/react-router'

export const Route = createFileRoute('/_authenticated/settings')({
  staticData: {
    pageName: 'Settings',
  },
  component: RouteComponent,
})

function RouteComponent() {
  // TODO: If user is administrator, redirect to overview

  return <div>Hello "/settings"!</div>
}
