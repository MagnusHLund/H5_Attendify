import { Outlet, createRootRoute, useNavigate } from '@tanstack/react-router'
import { Header, Footer } from '../components/layout'
import { logout } from '../features/auth/api/logout'
import { useErrorModal, useLoadingOverlay } from '../components/ui'
import { useTranslation } from '../lib/i18n'
import { queryClient } from '../lib/queryClient'

export const Route = createRootRoute({
  component: RootComponent,
})

function RootComponent() {
  const { t } = useTranslation()
  const { showError } = useErrorModal()
  const { runWithLoading } = useLoadingOverlay()
  const navigate = useNavigate()

  async function handleLogout() {
    try {
      await runWithLoading(() => logout())
      queryClient.removeQueries({ queryKey: ['student-access-code'] })
      navigate({ to: '/login' })
    } catch (error) {
      showError(
        error,
        t('error.logoutFailedTitle'),
        t('error.logoutFailedMessage'),
      )
    }
  }

  return (
    <div className="app-shell">
      <Header onLogout={handleLogout} />
      <main className="app-shell__content">
        <Outlet />
      </main>
      <Footer />
    </div>
  )
}
