import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import english from '../../translations/en-GB.json'
import danish from '../../translations/da-DK.json'

export type Locale = 'en-GB' | 'da-DK'

type TranslationObject = typeof english

type Join<K, P> = K extends string
  ? P extends string
    ? `${K}.${P}`
    : never
  : never

type TranslationKeys<T> = {
  [K in keyof T & string]: T[K] extends Record<string, unknown>
    ? Join<K, TranslationKeys<T[K]>>
    : K
}[keyof T & string]

export type TranslationKey = TranslationKeys<TranslationObject>

type TranslationParameters = Record<string, string | number>

const STORAGE_KEY = 'attendify.locale'
const DEFAULT_LOCALE: Locale = 'en-GB'

const translations: Record<Locale, TranslationObject> = {
  'en-GB': english,
  'da-DK': danish,
}

interface TranslationContextValue {
  locale: Locale
  setLocale: (locale: Locale) => void
  t: (key: TranslationKey, parameters?: TranslationParameters) => string
}

const TranslationContext = createContext<TranslationContextValue | null>(null)

function isLocale(value: string | null): value is Locale {
  return value === 'en-GB' || value === 'da-DK'
}

function getInitialLocale(): Locale {
  try {
    const storedLocale = localStorage.getItem(STORAGE_KEY)

    if (isLocale(storedLocale)) {
      return storedLocale
    }
  } catch (error) {
    console.warn('Unable to read the saved Attendify language.', error)
  }

  return navigator.language.toLowerCase().startsWith('da')
    ? 'da-DK'
    : DEFAULT_LOCALE
}

function getTranslation(
  translationObject: TranslationObject,
  key: TranslationKey,
): string {
  const value = key.split('.').reduce<unknown>((current, part) => {
    if (typeof current === 'object' && current !== null && part in current) {
      return (current as Record<string, unknown>)[part]
    }

    return undefined
  }, translationObject)

  if (typeof value !== 'string') {
    throw new Error(`Translation key "${key}" does not resolve to a string`)
  }

  return value
}

interface TranslationProviderProps {
  children: ReactNode
}

export function TranslationProvider({ children }: TranslationProviderProps) {
  const [locale, setLocaleState] = useState<Locale>(getInitialLocale)

  useEffect(() => {
    document.documentElement.lang = locale
  }, [locale])

  const setLocale = useCallback((newLocale: Locale) => {
    setLocaleState(newLocale)

    try {
      localStorage.setItem(STORAGE_KEY, newLocale)
    } catch (error) {
      console.warn('Unable to save the selected Attendify language.', error)
    }
  }, [])

  const t = useCallback(
    (key: TranslationKey, parameters: TranslationParameters = {}) => {
      const translation = getTranslation(translations[locale], key)

      return Object.entries(parameters).reduce(
        (result, [name, value]) =>
          result.replaceAll(`{${name}}`, String(value)),
        translation,
      )
    },
    [locale],
  )

  const contextValue = useMemo(
    () => ({ locale, setLocale, t }),
    [locale, setLocale, t],
  )

  return (
    <TranslationContext.Provider value={contextValue}>
      {children}
    </TranslationContext.Provider>
  )
}

export function useTranslation() {
  const context = useContext(TranslationContext)

  if (!context) {
    throw new Error('useTranslation must be used within a TranslationProvider')
  }

  return context
}
