import { useState } from 'react'
import './CopyField.scss'

interface CopyFieldProps {
  label?: string
  value: string
  error?: string
}

export function CopyField({ label, value, error }: CopyFieldProps) {
  const [copied, setCopied] = useState(false)

  async function handleCopy() {
    await navigator.clipboard.writeText(value)

    setCopied(true)

    setTimeout(() => {
      setCopied(false)
    }, 1500)
  }

  return (
    <div className="copy-field">
      {label && <span className="copy-field__label">{label}</span>}

      <div className="copy-field__container">
        <input className="copy-field__value" value={value} readOnly />

        <button
          type="button"
          className="copy-field__button"
          onClick={handleCopy}
          aria-label="Copy value"
        >
          {copied ? 'Copied' : 'Copy'}
        </button>
      </div>

      {error && <span className="copy-field__error">{error}</span>}
    </div>
  )
}
