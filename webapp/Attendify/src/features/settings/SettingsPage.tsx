import { FacePhotoForm } from './components/FacePhotoForm/FacePhotoForm'
import { StudentAccessCode } from './components/StudentAccessCode/StudentAccessCode'
import { LanguageSwitcher } from '../../components/ui'
import { PrivacyControls } from './components/PrivacyControls/PrivacyControls'
import { useTranslation } from '../../lib/i18n'
import './SettingsPage.scss'

export function SettingsPage() {
  const { t } = useTranslation()

  return (
    <div className="settings-page">
      <section className="settings-page__section">
        <FacePhotoForm />
      </section>

      <section className="settings-page__section settings-page__language">
        <div>
          <h2>{t('settings.languageTitle')}</h2>
          <p>{t('settings.languageDescription')}</p>
        </div>
        <LanguageSwitcher />
      </section>

      <section className="settings-page__section">
        <StudentAccessCode />
      </section>

      <section className="settings-page__section">
        <PrivacyControls />
      </section>
    </div>
  )
}
