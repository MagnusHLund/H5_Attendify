import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
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
  const pendingOperationsRef = useRef(0)
  const contentRef = useRef<HTMLDivElement>(null)
  const loadingStatusRef = useRef<HTMLDivElement>(null)
  const previouslyFocusedElementRef = useRef<HTMLElement | null>(null)
  const isLoading = pendingOperations > 0

  useEffect(() => {
    if (isLoading) {
      loadingStatusRef.current?.focus()
      return
    }

    const previouslyFocusedElement = previouslyFocusedElementRef.current
    previouslyFocusedElementRef.current = null

    if (
      previouslyFocusedElement?.isConnected &&
      !previouslyFocusedElement.closest('[inert]')
    ) {
      previouslyFocusedElement.focus()
    }
  }, [isLoading])

  const runWithLoading = useCallback(
    async <T,>(operation: () => Promise<T>): Promise<T> => {
      if (pendingOperationsRef.current === 0) {
        const activeElement = document.activeElement

        if (
          activeElement instanceof HTMLElement &&
          contentRef.current?.contains(activeElement)
        ) {
          previouslyFocusedElementRef.current = activeElement
        }
      }

      pendingOperationsRef.current += 1
      setPendingOperations(pendingOperationsRef.current)

      try {
        return await operation()
      } finally {
        pendingOperationsRef.current = Math.max(pendingOperationsRef.current - 1, 0)
        setPendingOperations(pendingOperationsRef.current)
      }
    },
    [],
  )

  const contextValue = useMemo(() => ({ runWithLoading }), [runWithLoading])

  return (
    <LoadingOverlayContext.Provider value={contextValue}>
      <div
        ref={contentRef}
        className="loading-overlay-content"
        inert={isLoading}
      >
        {children}
      </div>
      {isLoading && (
        <div className="loading-overlay" aria-busy="true">
          <div
            ref={loadingStatusRef}
            className="loading-overlay__status"
            role="status"
            aria-live="polite"
            aria-label={t('common.loading')}
            tabIndex={-1}
          >
            <Spinner size="large" decorative />
          </div>
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
