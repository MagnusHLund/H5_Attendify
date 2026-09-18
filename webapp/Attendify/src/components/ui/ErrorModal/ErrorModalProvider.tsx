import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import { ErrorModal } from './ErrorModal'
import { useTranslation } from '../../../lib/i18n'

interface ErrorDetails {
  message: string
  title?: string
}

interface ErrorModalContextValue {
  showError: (error: unknown, title?: string) => void
}

const ErrorModalContext = createContext<ErrorModalContextValue | null>(null)

interface ErrorModalProviderProps {
  children: ReactNode
}

export function ErrorModalProvider({ children }: ErrorModalProviderProps) {
  const { t } = useTranslation()
  const [errorDetails, setErrorDetails] = useState<ErrorDetails | null>(null)

  const showError = useCallback((error: unknown, title?: string) => {
    const message =
      error instanceof Error ? error.message : t('error.unexpected')

    setErrorDetails({ message, title })
  }, [t])

  const contextValue = useMemo(() => ({ showError }), [showError])

  return (
    <ErrorModalContext.Provider value={contextValue}>
      {children}
      <ErrorModal
        isOpen={errorDetails !== null}
        title={errorDetails?.title}
        message={errorDetails?.message ?? ''}
        onClose={() => setErrorDetails(null)}
      />
    </ErrorModalContext.Provider>
  )
}

export function useErrorModal() {
  const context = useContext(ErrorModalContext)

  if (!context) {
    throw new Error('useErrorModal must be used within an ErrorModalProvider')
  }

  return context
}
