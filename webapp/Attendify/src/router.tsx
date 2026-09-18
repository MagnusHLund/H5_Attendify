import { createRouter } from '@tanstack/react-router'
import { routeTree } from './routeTree.gen'
import type { TranslationKey } from './lib/i18n'

export const router = createRouter({
  routeTree,
})

declare module '@tanstack/react-router' {
  interface Register {
    router: typeof router
  }

  interface StaticDataRouteOption {
    pageNameKey?: TranslationKey
  }
}
