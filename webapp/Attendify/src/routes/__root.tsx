import { Outlet, createRootRoute } from '@tanstack/react-router'
import { Header, Footer } from '../components/layout'

export const Route = createRootRoute({
  component: RootComponent,
})

function RootComponent() {
  // TODO: User role and logout logic should be implemented.

  return (
    <div className="app-shell">
      <Header userRole="student" onLogout={() => {}} />
      <main className="app-shell__content">
        <Outlet />
      </main>
      <Footer />
    </div>
  )
}
