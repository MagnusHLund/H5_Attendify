import { useId, useState } from 'react'
import { useTranslation } from '../../../lib/i18n'
import './CopyField.scss'

interface CopyFieldProps {
  label?: string
  value: string
  error?: string
}

export function CopyField({ label, value, error }: CopyFieldProps) {
  const { t } = useTranslation()
  const [copied, setCopied] = useState(false)
  const inputId = useId()
  const errorId = `${inputId}-error`

  async function handleCopy() {
    await navigator.clipboard.writeText(value)

    setCopied(true)

    setTimeout(() => {
      setCopied(false)
    }, 1500)
  }

  return (
    <div className="copy-field">
      {label && (
        <label className="copy-field__label" htmlFor={inputId}>
          {label}
        </label>
      )}

      <div className="copy-field__container">
        <input
          id={inputId}
          className="copy-field__value"
          value={value}
          readOnly
          aria-invalid={!!error}
          aria-describedby={error ? errorId : undefined}
        />

        <button
          type="button"
          className="copy-field__button"
          onClick={handleCopy}
          aria-label={t('accessibility.copyValue')}
        >
          {copied ? t('common.copied') : t('common.copy')}
        </button>
      </div>

      {error && (
        <span id={errorId} className="copy-field__error">
          {error}
        </span>
      )}
    </div>
  )
}
