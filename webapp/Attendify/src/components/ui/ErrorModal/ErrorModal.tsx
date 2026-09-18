import { useEffect, useId, useRef, type ReactNode } from 'react'
import { createPortal } from 'react-dom'
import { Button } from '../Button/Button'
import { useTranslation } from '../../../lib/i18n'
import './ErrorModal.scss'

interface ErrorModalProps {
  isOpen: boolean
  message: ReactNode
  onClose: () => void
  title?: string
}

export function ErrorModal({
  isOpen,
  message,
  onClose,
  title,
}: ErrorModalProps) {
  const { t } = useTranslation()
  const titleId = useId()
  const messageId = useId()
  const dialogRef = useRef<HTMLElement>(null)

  useEffect(() => {
    if (!isOpen) {
      return
    }

    const previouslyFocusedElement = document.activeElement
    const previousOverflow = document.body.style.overflow

    document.body.style.overflow = 'hidden'
    dialogRef.current?.focus()

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        onClose()
        return
      }

      if (event.key !== 'Tab' || !dialogRef.current) {
        return
      }

      const focusableElements = dialogRef.current.querySelectorAll<HTMLElement>(
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

      if (previouslyFocusedElement instanceof HTMLElement) {
        previouslyFocusedElement.focus()
      }
    }
  }, [isOpen, onClose])

  if (!isOpen) {
    return null
  }

  return createPortal(
    <div
      className="error-modal"
      role="presentation"
      onMouseDown={(event) => {
        if (event.target === event.currentTarget) {
          onClose()
        }
      }}
    >
      <section
        ref={dialogRef}
        className="error-modal__dialog"
        role="alertdialog"
        aria-modal="true"
        aria-labelledby={titleId}
        aria-describedby={messageId}
        tabIndex={-1}
      >
        <div className="error-modal__icon" aria-hidden="true">
          !
        </div>

        <h2 className="error-modal__title" id={titleId}>
          {title ?? t('error.defaultTitle')}
        </h2>

        <div className="error-modal__message" id={messageId}>
          {message}
        </div>

        <Button type="button" variant="danger" onClick={onClose}>
          {t('common.close')}
        </Button>
      </section>
    </div>,
    document.body,
  )
}
