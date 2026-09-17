import type { ChangeEvent } from 'react'
import './FileInput.scss'

interface FileInputProps {
  label?: string
  accept?: string
  error?: string
  disabled?: boolean
  onChange?: (file: File | null) => void
}

export function FileInput({
  label,
  accept,
  error,
  disabled = false,
  onChange,
}: FileInputProps) {
  function handleChange(event: ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0] ?? null

    onChange?.(file)
  }

  return (
    <div className="file-input">
      {label && <span className="file-input__label">{label}</span>}

      <label
        className={`file-input__dropzone ${
          disabled ? 'file-input__dropzone--disabled' : ''
        }`}
      >
        <input
          type="file"
          accept={accept}
          disabled={disabled}
          onChange={handleChange}
        />

        <span className="file-input__icon">↑</span>

        <span className="file-input__text">
          <strong>Choose a file</strong>
          <small>or drag and drop it here</small>
        </span>
      </label>

      {error && <span className="file-input__error">{error}</span>}
    </div>
  )
}
