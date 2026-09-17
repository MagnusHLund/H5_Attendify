import type { InputHTMLAttributes } from 'react'
import './TextInput.scss'

interface TextInputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string
  error?: string
}

export function TextInput({ label, error, id, ...props }: TextInputProps) {
  return (
    <div className="input-field">
      {label && (
        <label htmlFor={id} className="input-field__label">
          {label}
        </label>
      )}

      <input
        id={id}
        className={`input-field__control ${
          error ? 'input-field__control--error' : ''
        }`}
        {...props}
      />

      {error && <span className="input-field__error">{error}</span>}
    </div>
  )
}
