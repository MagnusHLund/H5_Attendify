import { Link } from '@tanstack/react-router'
import { useEffect, useRef, type MouseEvent, type RefObject } from 'react'
import { Image } from '../../ui'
import './Sidebar.scss'

interface SidebarProps {
  isSettingsPage: boolean
  menuButtonRef: RefObject<HTMLButtonElement | null>
  onClose: () => void
  onLogout: () => void
  userRole: 'student' | 'administrator'
}

export function Sidebar({
  isSettingsPage,
  menuButtonRef,
  onClose,
  onLogout,
  userRole,
}: SidebarProps) {
  const sidebarRef = useRef<HTMLElement>(null)
  const shouldRestoreFocusRef = useRef(false)

  useEffect(() => {
    const previousOverflow = document.body.style.overflow

    document.body.style.overflow = 'hidden'
    sidebarRef.current?.focus()

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        shouldRestoreFocusRef.current = true
        onClose()
        return
      }

      if (event.key !== 'Tab' || !sidebarRef.current) {
        return
      }

      const focusableElements =
        sidebarRef.current.querySelectorAll<HTMLElement>(
          'a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])',
        )

      if (focusableElements.length === 0) {
        event.preventDefault()
        return
      }

      const firstElement = focusableElements[0]
      const lastElement = focusableElements[focusableElements.length - 1]

      if (event.shiftKey && document.activeElement === firstElement) {
        event.preventDefault()
        lastElement.focus()
      } else if (!event.shiftKey && document.activeElement === lastElement) {
        event.preventDefault()
        firstElement.focus()
      }
    }

    document.addEventListener('keydown', handleKeyDown)

    return () => {
      document.removeEventListener('keydown', handleKeyDown)
      document.body.style.overflow = previousOverflow

      if (shouldRestoreFocusRef.current) {
        menuButtonRef.current?.focus()
      }
    }
  }, [menuButtonRef, onClose])

  const closeFromButton = (event: MouseEvent<HTMLButtonElement>) => {
    shouldRestoreFocusRef.current = event.detail === 0
    onClose()
  }

  return (
    <div
      className="sidebar__backdrop"
      role="presentation"
      onMouseDown={(event) => {
        if (event.target === event.currentTarget) {
          onClose()
        }
      }}
    >
      <nav
        ref={sidebarRef}
        id="navigation"
        className="sidebar"
        aria-label="User navigation"
        tabIndex={-1}
      >
        <div className="sidebar__heading">
          <div className="sidebar__brand">
            <Link className="sidebar__logo" to="/overview" onClick={onClose}>
              <Image src="/Attendify-small.png" alt="Attendify" />
            </Link>
            <span>Menu</span>
          </div>

          <button
            className="sidebar__close"
            type="button"
            aria-label="Close navigation menu"
            onClick={closeFromButton}
          >
            <span aria-hidden="true" />
          </button>
        </div>

        <div className="sidebar__actions">
          {userRole === 'student' && (
            <Link
              className="sidebar__action"
              to={isSettingsPage ? '/overview' : '/settings'}
              onClick={onClose}
            >
              {isSettingsPage ? 'Back to Overview' : 'Settings'}
            </Link>
          )}

          <button
            className="sidebar__action sidebar__action--logout"
            type="button"
            onClick={() => {
              onClose()
              onLogout()
            }}
          >
            Log out
          </button>
        </div>
      </nav>
    </div>
  )
}
