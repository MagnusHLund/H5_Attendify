import { useEffect, useId, useRef, type ReactNode } from 'react'
import { createPortal } from 'react-dom'
import { Button } from '../Button/Button'
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
  title = 'Something went wrong',
}: ErrorModalProps) {
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
          {title}
        </h2>
        <div className="error-modal__message" id={messageId}>
          {message}
        </div>
        <Button
          type="button"
          variant="danger"
          onClick={onClose}
        >
          Close
        </Button>
      </section>
    </div>,
    document.body,
  )
}
