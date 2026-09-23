import { Outlet, createRootRoute } from '@tanstack/react-router'
import { Header, Footer } from '../components/layout'
import { logout } from '../features/auth/api/logout'
import { ErrorModal, useErrorModal } from '../components/ui'
import { useTranslation } from '../lib/i18n'

export const Route = createRootRoute({
  component: RootComponent,
})

function RootComponent() {
  const { t } = useTranslation()
  const { showError } = useErrorModal()

  async function handleLogout() {
    try {
      await logout()
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
