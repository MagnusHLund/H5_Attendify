import { useId } from 'react'
import type { InputHTMLAttributes } from 'react'
import './TextInput.scss'

interface TextInputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string
  error?: string
}

export function TextInput({
  label,
  error,
  id,
  className,
  ...props
}: TextInputProps) {
  const generatedId = useId()
  const inputId = id ?? generatedId
  const errorId = `${inputId}-error`

  return (
    <div className="input-field">
      {label && (
        <label htmlFor={inputId} className="input-field__label">
          {label}
        </label>
      )}

      <input
        id={inputId}
        className={`input-field__control ${
          error ? 'input-field__control--error' : ''
        } ${className ?? ''}`}
        aria-invalid={error ? true : undefined}
        aria-describedby={error ? errorId : undefined}
        {...props}
      />

      {error && (
        <span id={errorId} className="input-field__error">
          {error}
        </span>
      )}
    </div>
  )
}
