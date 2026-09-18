import {
  useEffect,
  useId,
  useState,
  type ChangeEvent,
  type DragEvent,
} from 'react'
import { Image } from '../Image/Image'
import { useTranslation } from '../../../lib/i18n'
import './FileInput.scss'

interface FileInputProps {
  label?: string
  accept?: string
  error?: string
  disabled?: boolean
  onChange?: (file: File | null) => void
}

function isAcceptedFile(file: File, accept?: string): boolean {
  if (!accept) {
    return true
  }

  const acceptedTypes = accept
    .split(',')
    .map((type) => type.trim().toLowerCase())
    .filter(Boolean)

  if (acceptedTypes.length === 0) {
    return true
  }

  const fileType = file.type.toLowerCase()
  const extensionSeparatorIndex = file.name.lastIndexOf('.')
  const fileExtension =
    extensionSeparatorIndex >= 0
      ? file.name.slice(extensionSeparatorIndex).toLowerCase()
      : ''

  return acceptedTypes.some((acceptedType) => {
    if (acceptedType === '*/*') {
      return true
    }

    if (acceptedType.endsWith('/*')) {
      return fileType.startsWith(acceptedType.slice(0, -1))
    }

    if (acceptedType.startsWith('.')) {
      return fileExtension === acceptedType
    }

    return fileType === acceptedType
  })
}

export function FileInput({
  label,
  accept,
  error,
  disabled = false,
  onChange,
}: FileInputProps) {
  const { t } = useTranslation()
  const inputId = useId()
  const errorId = `${inputId}-error`
  const [selectedFile, setSelectedFile] = useState<File | null>(null)
  const [previewUrl, setPreviewUrl] = useState<string | null>(null)
  const [internalError, setInternalError] = useState<string>()
  const displayError = error ?? internalError

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
    if (file && !isAcceptedFile(file, accept)) {
      setInternalError(
        accept?.toLowerCase().includes('image/')
          ? t('file.invalidImage')
          : t('file.invalidType'),
      )
      return
    }

    setInternalError(undefined)
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
          displayError ? 'file-input__dropzone--error' : '',
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
          aria-invalid={displayError ? true : undefined}
          aria-describedby={displayError ? errorId : undefined}
          onChange={handleChange}
        />

        {previewUrl ? (
          <Image
            className="file-input__preview"
            src={previewUrl}
            alt={t('file.preview', {
              name: selectedFile?.name ?? t('file.choosePicture'),
            })}
          />
        ) : (
          <span className="file-input__icon" aria-hidden="true">
            ↑
          </span>
        )}

        <span className="file-input__text">
          <strong>{selectedFile?.name ?? t('file.choosePicture')}</strong>
          <small>
            {selectedFile ? t('file.replace') : t('file.dragDrop')}
          </small>
        </span>
      </label>

      {displayError && (
        <span id={errorId} className="file-input__error" role="alert">
          {displayError}
        </span>
      )}
    </div>
  )
}
