import {
  useEffect,
  useId,
  useState,
  type ChangeEvent,
  type DragEvent,
} from 'react'
import { Image } from '../Image/Image'
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
  const inputId = useId()
  const errorId = `${inputId}-error`
  const [selectedFile, setSelectedFile] = useState<File | null>(null)
  const [previewUrl, setPreviewUrl] = useState<string | null>(null)

  useEffect(() => {
    if (!selectedFile?.type.startsWith('image/')) {
      setPreviewUrl(null)
      return
    }

    const objectUrl = URL.createObjectURL(selectedFile)
    setPreviewUrl(objectUrl)

    return () => URL.revokeObjectURL(objectUrl)
  }, [selectedFile])

  function selectFile(file: File | null) {
    setSelectedFile(file)
    onChange?.(file)
  }

  function handleChange(event: ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0] ?? null

    selectFile(file)
  }

  function handleDragOver(event: DragEvent<HTMLLabelElement>) {
    if (disabled) {
      return
    }

    event.preventDefault()
  }

  function handleDrop(event: DragEvent<HTMLLabelElement>) {
    if (disabled) {
      return
    }

    event.preventDefault()

    const file = event.dataTransfer.files[0] ?? null

    selectFile(file)
  }

  return (
    <div className="file-input">
      {label && (
        <label className="file-input__label" htmlFor={inputId}>
          {label}
        </label>
      )}

      <label
        className={[
          'file-input__dropzone',
          disabled ? 'file-input__dropzone--disabled' : '',
          previewUrl ? 'file-input__dropzone--selected' : '',
          error ? 'file-input__dropzone--error' : '',
        ].join(' ')}
        htmlFor={inputId}
        onDragOver={handleDragOver}
        onDrop={handleDrop}
      >
        <input
          id={inputId}
          type="file"
          accept={accept}
          disabled={disabled}
          aria-invalid={error ? true : undefined}
          aria-describedby={error ? errorId : undefined}
          onChange={handleChange}
        />

        {previewUrl ? (
          <Image
            className="file-input__preview"
            src={previewUrl}
            alt={`Preview of ${selectedFile?.name ?? 'selected image'}`}
          />
        ) : (
          <span className="file-input__icon" aria-hidden="true">
            ↑
          </span>
        )}

        <span className="file-input__text">
          <strong>{selectedFile?.name ?? 'Choose a picture'}</strong>
          <small>
            {selectedFile ? 'Click to replace' : 'or drag and drop it here'}
          </small>
        </span>
      </label>

      {error && (
        <span id={errorId} className="file-input__error">
          {error}
        </span>
      )}
    </div>
  )
}
