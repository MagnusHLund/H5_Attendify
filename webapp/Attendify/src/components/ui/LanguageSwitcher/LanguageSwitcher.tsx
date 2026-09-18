import { useTranslation, type Locale } from '../../../lib/i18n'
import { Image } from '../Image/Image'
import './LanguageSwitcher.scss'

interface LanguageSwitcherProps {
  compact?: boolean
}

const languages: Array<{
  locale: Locale
  flag: string
  labelKey: 'language.english' | 'language.danish'
}> = [
  {
    locale: 'en-GB',
    flag: '/external/flags/en_GB.png',
    labelKey: 'language.english',
  },
  {
    locale: 'da-DK',
    flag: '/external/flags/da_DK.png',
    labelKey: 'language.danish',
  },
]

export function LanguageSwitcher({
  compact = false,
}: LanguageSwitcherProps) {
  const { locale, setLocale, t } = useTranslation()

  return (
    <div
      className={`language-switcher ${
        compact ? 'language-switcher--compact' : ''
      }`}
      role="group"
      aria-label={t('language.label')}
    >
      {languages.map((language) => {
        const label = t(language.labelKey)

        return (
          <button
            key={language.locale}
            className={`language-switcher__option ${
              locale === language.locale
                ? 'language-switcher__option--selected'
                : ''
            }`}
            type="button"
            aria-label={t('language.changeTo', { language: label })}
            aria-pressed={locale === language.locale}
            onClick={() => setLocale(language.locale)}
          >
            <Image src={language.flag} alt="" />
            {!compact && <span>{label}</span>}
          </button>
        )
      })}
    </div>
  )
}
