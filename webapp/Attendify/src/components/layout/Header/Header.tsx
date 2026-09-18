import {
  Link,
  useLocation,
  useMatches,
  useNavigate,
} from '@tanstack/react-router'
import { useRef, useState } from 'react'
import { Image } from '../../ui'
import { Sidebar } from '../Sidebar/Sidebar'

import './Header.scss'

type HeaderProps = {
  userRole: 'student' | 'administrator'
  onLogout: () => void
}

export function Header({ userRole, onLogout }: HeaderProps) {
  const location = useLocation()
  const matches = useMatches()
  const navigate = useNavigate()
  const [isMenuOpen, setIsMenuOpen] = useState(false)
  const menuButtonRef = useRef<HTMLButtonElement>(null)

  const isSettingsPage = location.pathname === '/settings'

  const pageName =
    [...matches].reverse().find((match) => match.staticData.pageName)
      ?.staticData.pageName ?? 'Attendify'

  const handleAction = () => {
    if (userRole === 'administrator') {
      onLogout()
      return
    }

    navigate({
      to: isSettingsPage ? '/overview' : '/settings',
    })
  }

  const actionLabel =
    userRole === 'administrator'
      ? 'Log out'
      : isSettingsPage
        ? 'Back to Overview'
        : 'Settings'

  if (!isSettingsPage && location.pathname !== '/overview') {
    return null
  }

  return (
    <header className="header">
      <Link className="header__logo" to="/overview">
        <Image src="/Attendify-large.png" alt="Attendify" />
      </Link>

      <h1 className="header__title">{pageName}</h1>

      <div className="header__actions">
        <button className="header__action" type="button" onClick={handleAction}>
          {actionLabel}
        </button>
      </div>

      <button
        ref={menuButtonRef}
        className="header__menu-button"
        type="button"
        aria-label={
          isMenuOpen ? 'Close navigation menu' : 'Open navigation menu'
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
