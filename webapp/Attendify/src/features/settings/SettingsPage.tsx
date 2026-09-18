import { FacePhotoForm } from './components/FacePhotoForm/FacePhotoForm'
import { StudentAccessCode } from './components/StudentAccessCode/StudentAccessCode'
import './SettingsPage.scss'

export function SettingsPage() {
  return (
    <div className="settings-page">
      <section className="settings-page__section">
        <h1 className="settings-page__title">Settings</h1>

        <FacePhotoForm />
      </section>

      <section className="settings-page__section">
        <StudentAccessCode />
      </section>
    </div>
  )
}
