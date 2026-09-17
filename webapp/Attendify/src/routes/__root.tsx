import * as React from 'react'
import { Outlet, createRootRoute } from '@tanstack/react-router'
import { Header, Footer } from '../components/layout'

export const Route = createRootRoute({
  component: RootComponent,
})

function RootComponent() {
  // TODO: User role and logout logic should be implemented.

  return (
    <React.Fragment>
      <Header userRole="administrator" onLogout={() => {}} />
      <Outlet />
      <Footer />
    </React.Fragment>
  )
}
