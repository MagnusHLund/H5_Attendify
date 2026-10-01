import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { Button, TextInput } from '../../../../components/ui'
import { fetchApi } from '../../../../lib/api/client'
import { useTranslation } from '../../../../lib/i18n'
import './PrivacyControls.scss'

type PrivacyAction = 'export' | 'preference' | 'delete'

export function PrivacyControls() {
  const { t } = useTranslation()
  const [enabled, setEnabled] = useState<boolean | null>(null)
  const [preferenceLoading, setPreferenceLoading] = useState(true)
  const [preferenceLoadFailed, setPreferenceLoadFailed] = useState(false)
  const [preferenceRetry, setPreferenceRetry] = useState(0)
  const [action, setAction] = useState<PrivacyAction | null>(null)
  const [message, setMessage] = useState('')
  const [password, setPassword] = useState('')

  useEffect(() => {
    let active = true
    fetchApi('/api/settings/attendance-preference', { credentials: 'include' })
      .then(async (response) => {
        if (!response.ok) throw new Error()
        const body: unknown = await response.json()
        if (
          typeof body !== 'object' ||
          body === null ||
          !('enabled' in body) ||
          typeof body.enabled !== 'boolean'
        ) {
          throw new Error()
        }
        if (active) setEnabled(body.enabled)
      })
      .catch(() => {
        if (active) {
          setPreferenceLoadFailed(true)
          setMessage(t('settings.privacyLoadFailed'))
        }
      })
      .finally(() => {
        if (active) setPreferenceLoading(false)
      })
    return () => { active = false }
  }, [preferenceRetry, t])

  async function updateAttendance(nextEnabled: boolean) {
    setAction('preference')
    setMessage('')
    try {
      const response = await fetchApi('/api/settings/attendance-preference', {
        method: 'PATCH',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ enabled: nextEnabled }),
      })
      if (!response.ok) {
        if (response.status === 409) throw new Error(t('settings.addPhotosBeforeEnabling'))
        throw new Error(t('settings.preferenceFailed'))
      }
      setEnabled(nextEnabled)
      setMessage(nextEnabled ? t('settings.attendanceEnabled') : t('settings.attendanceDisabled'))
    } catch (error) {
      setMessage(error instanceof Error ? error.message : t('settings.preferenceFailed'))
    } finally {
      setAction(null)
    }
  }

  async function exportPersonalData() {
    setAction('export')
    setMessage('')
    try {
      const response = await fetchApi('/api/settings/personal-data', { credentials: 'include' })
      if (!response.ok) throw new Error(t('settings.exportFailed'))
      const blob = await response.blob()
      const url = URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = 'attendify-personal-data.json'
      link.hidden = true
      document.body.append(link)
      link.click()
      link.remove()
      window.setTimeout(() => URL.revokeObjectURL(url), 1000)
      setMessage(t('settings.exportReady'))
    } catch {
      setMessage(t('settings.exportFailed'))
    } finally {
      setAction(null)
    }
  }

  async function deleteAccount(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!window.confirm(t('settings.deleteConfirm'))) return
    setAction('delete')
    setMessage('')
    try {
      const response = await fetchApi('/api/settings/account', {
        method: 'DELETE',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ currentPassword: password }),
      })
      if (!response.ok) {
        if (response.status === 400) {
          throw new Error(t('settings.deleteInvalidPassword'))
        }
        throw new Error(t('settings.deleteFailed'))
      }
      window.location.assign('/login')
    } catch (error) {
      setMessage(error instanceof Error ? error.message : t('settings.deleteFailed'))
    } finally {
      setAction(null)
    }
  }

  const busy = action !== null

  return (
    <section className="privacy-controls">
      <div className="privacy-controls__header">
        <h2>{t('settings.privacyTitle')}</h2>
        <p>{t('settings.privacyDescription')}</p>
      </div>
      <div className="privacy-controls__preference">
        <label className="privacy-controls__toggle">
          <input
            type="checkbox"
            checked={enabled ?? false}
            disabled={enabled === null || busy}
            onChange={(event) => void updateAttendance(event.target.checked)}
          />
          <span>{t('settings.attendancePreference')}</span>
        </label>
        <p>{t('settings.attendancePreferenceDescription')}</p>
        {preferenceLoadFailed && (
          <Button
            type="button"
            variant="secondary"
            onClick={() => {
              setMessage('')
              setPreferenceLoadFailed(false)
              setPreferenceLoading(true)
              setPreferenceRetry((retry) => retry + 1)
            }}
            loading={preferenceLoading}
            disabled={busy}
          >
            {t('settings.retryPrivacyLoad')}
          </Button>
        )}
      </div>
      <div className="privacy-controls__export">
        <p>{t('settings.exportDescription')}</p>
        <Button
          type="button"
          variant="secondary"
          onClick={exportPersonalData}
          loading={action === 'export'}
          disabled={busy}
        >
          {t('settings.downloadPersonalData')}
        </Button>
      </div>
      {message && <p role="status" className="privacy-controls__message">{message}</p>}
      <form className="privacy-controls__delete" onSubmit={(event) => void deleteAccount(event)}>
        <h3>{t('settings.deleteAccountTitle')}</h3>
        <p>{t('settings.deleteAccountDescription')}</p>
        <TextInput
          type="password"
          autoComplete="current-password"
          label={t('settings.currentPassword')}
          value={password}
          onChange={(event) => setPassword(event.target.value)}
          required
          disabled={busy}
        />
        <Button
          type="submit"
          variant="danger"
          loading={action === 'delete'}
          disabled={!password || busy}
        >
          {t('settings.deleteAccount')}
        </Button>
      </form>
    </section>
  )
}
