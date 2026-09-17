import { useId, useState } from 'react'
import './CopyField.scss'

interface CopyFieldProps {
  label?: string
  value: string
  error?: string
}

export function CopyField({ label, value, error }: CopyFieldProps) {
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
          aria-label="Copy value"
        >
          {copied ? 'Copied' : 'Copy'}
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
