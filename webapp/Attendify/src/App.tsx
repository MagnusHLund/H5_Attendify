import { RouterProvider } from '@tanstack/react-router'
import { QueryClientProvider } from '@tanstack/react-query'

import { router } from './router'
import { queryClient } from './lib/queryClient'
import { ErrorModalProvider } from './components/ui'

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ErrorModalProvider>
        <RouterProvider router={router} />
      </ErrorModalProvider>
    </QueryClientProvider>
  )
}

export default App
