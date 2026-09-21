import {
  Link,
  useLocation,
  useMatches,
  useNavigate,
} from '@tanstack/react-router'
import { useRef, useState } from 'react'
import { Image, LanguageSwitcher } from '../../ui'
import { useTranslation } from '../../../lib/i18n'
import { Sidebar } from '../Sidebar/Sidebar'
import { useCurrentUser } from '../../../features/auth/hooks/useCurrentUser'

import './Header.scss'

type HeaderProps = {
  onLogout: () => void
}

export function Header({ onLogout }: HeaderProps) {
  const location = useLocation()
  const matches = useMatches()
  const navigate = useNavigate()
  const { t } = useTranslation()
  const [isMenuOpen, setIsMenuOpen] = useState(false)
  const menuButtonRef = useRef<HTMLButtonElement>(null)

  const isSettingsPage = location.pathname === '/settings'
  const isOverviewPage = location.pathname === '/overview'

  const { data: user } = useCurrentUser(isSettingsPage || isOverviewPage)
  const userRole = user?.role ?? 'student'

  const pageNameKey = [...matches]
    .reverse()
    .find((match) => match.staticData.pageNameKey)?.staticData.pageNameKey
  const pageName = pageNameKey ? t(pageNameKey) : t('common.attendifyLogo')

  const handleAction = () => {
    if (userRole === 'school_administrator') {
      onLogout()
      return
    }

    navigate({
      to: isSettingsPage ? '/overview' : '/settings',
    })
  }

  const actionLabel =
    userRole === 'school_administrator'
      ? t('navigation.logout')
      : t('navigation.settings')

  if (!isSettingsPage && !isOverviewPage) {
    return null
  }

  return (
    <header className="header">
      <Link className="header__logo" to="/overview">
        <Image
          src="/internal/logos/Attendify-large.png"
          alt={t('common.attendifyLogo')}
        />
      </Link>

      <h1 className="header__title">{pageName}</h1>

      <div className="header__actions">
        {userRole === 'school_administrator' && <LanguageSwitcher compact />}
        {(userRole === 'school_administrator' || !isSettingsPage) && (
          <button
            className="header__action"
            type="button"
            onClick={handleAction}
          >
            {actionLabel}
          </button>
        )}
      </div>

      <button
        ref={menuButtonRef}
        className="header__menu-button"
        type="button"
        aria-label={
          isMenuOpen ? t('navigation.closeMenu') : t('navigation.openMenu')
        }
        aria-controls="mobile-navigation"
        aria-expanded={isMenuOpen}
        onClick={() => setIsMenuOpen((isOpen) => !isOpen)}
      >
        <span aria-hidden="true" />
        <span aria-hidden="true" />
        <span aria-hidden="true" />
      </button>

      {isMenuOpen && (
        <Sidebar
          isSettingsPage={isSettingsPage}
          menuButtonRef={menuButtonRef}
          userRole={userRole}
          onClose={() => setIsMenuOpen(false)}
          onLogout={onLogout}
        />
      )}
    </header>
  )
}
