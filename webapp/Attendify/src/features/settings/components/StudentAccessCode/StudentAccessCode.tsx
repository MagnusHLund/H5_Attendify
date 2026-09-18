import { useEffect, useRef } from 'react'
import { CopyField, Spinner, useErrorModal } from '../../../../components/ui'
import { useStudentAccessCode } from '../../hooks/useStudentAccessCode'
import { useTranslation } from '../../../../lib/i18n'
import './StudentAccessCode.scss'

export function StudentAccessCode() {
  const { data, error, isLoading } = useStudentAccessCode()
  const { showError } = useErrorModal()
  const { t } = useTranslation()

  const handledErrorRef = useRef<Error | null>(null)

  useEffect(() => {
    if (!error || handledErrorRef.current === error) {
      return
    }

    handledErrorRef.current = error

    showError(
      new Error(t('error.accessCodeMessage')),
      t('error.accessCodeTitle'),
    )
  }, [error, showError, t])

  return (
    <div className="student-access-code">
      <div className="student-access-code__header">
        <h2>{t('settings.accessCodeTitle')}</h2>
        <p>{t('settings.accessCodeDescription')}</p>
      </div>

      {isLoading && (
        <div className="student-access-code__status">
          <Spinner label={t('settings.loadingAccessCode')} />
          <span>{t('settings.loadingAccessCode')}</span>
        </div>
      )}

      {data && <CopyField label={t('settings.accessCode')} value={data.code} />}
    </div>
  )
}
