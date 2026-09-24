import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import { useTranslation } from '../../../lib/i18n'
import { Spinner } from '../Spinner/Spinner'
import './LoadingOverlayProvider.scss'

interface LoadingOverlayContextValue {
  runWithLoading: <T>(operation: () => Promise<T>) => Promise<T>
}

const LoadingOverlayContext =
  createContext<LoadingOverlayContextValue | null>(null)

interface LoadingOverlayProviderProps {
  children: ReactNode
}

export function LoadingOverlayProvider({
  children,
}: LoadingOverlayProviderProps) {
  const { t } = useTranslation()
  const [pendingOperations, setPendingOperations] = useState(0)

  const runWithLoading = useCallback(
    async <T,>(operation: () => Promise<T>): Promise<T> => {
      setPendingOperations((count) => count + 1)

      try {
        return await operation()
      } finally {
        setPendingOperations((count) => Math.max(count - 1, 0))
      }
    },
    [],
  )

  const contextValue = useMemo(() => ({ runWithLoading }), [runWithLoading])

  return (
    <LoadingOverlayContext.Provider value={contextValue}>
      {children}
      {pendingOperations > 0 && (
        <div className="loading-overlay" aria-busy="true">
          <Spinner size="large" label={t('common.loading')} />
        </div>
      )}
    </LoadingOverlayContext.Provider>
  )
}

export function useLoadingOverlay() {
  const context = useContext(LoadingOverlayContext)

  if (!context) {
    throw new Error(
      'useLoadingOverlay must be used within a LoadingOverlayProvider',
    )
  }

  return context
}
