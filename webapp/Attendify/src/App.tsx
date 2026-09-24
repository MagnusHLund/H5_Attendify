import { RouterProvider } from '@tanstack/react-router'
import { QueryClientProvider } from '@tanstack/react-query'

import { router } from './router'
import { queryClient } from './lib/queryClient'
import { ErrorModalProvider, LoadingOverlayProvider } from './components/ui'
import { TranslationProvider } from './lib/i18n'

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <TranslationProvider>
        <ErrorModalProvider>
          <LoadingOverlayProvider>
            <RouterProvider router={router} />
          </LoadingOverlayProvider>
        </ErrorModalProvider>
      </TranslationProvider>
    </QueryClientProvider>
  )
}

export default App
