import { useState } from 'react'
import { useForm } from '@tanstack/react-form'
import {
  Button,
  FileInput,
  useErrorModal,
  useLoadingOverlay,
} from '../../../../components/ui'
import { useUpdateFacePhotos } from '../../hooks/useUpdateFacePhotos'
import { useTranslation } from '../../../../lib/i18n'
import './FacePhotoForm.scss'
import { fileToBase64 } from '../../../../lib/encoding/base64'

export function FacePhotoForm() {
  const [showSuccess, setShowSuccess] = useState(false)
  const [fileInputVersion, setFileInputVersion] = useState(0)
  const updateFacePhotos = useUpdateFacePhotos()
  const { showError } = useErrorModal()
  const { runWithLoading } = useLoadingOverlay()
  const { t } = useTranslation()

  const form = useForm({
    defaultValues: {
      straightPhoto: null as File | null,
      leftPhoto: null as File | null,
      rightPhoto: null as File | null,
    },

    onSubmit: async ({ value }) => {
      setShowSuccess(false)

      try {
        await runWithLoading(async () => {
          const photos = {
            straightPhoto: await fileToBase64(value.straightPhoto!),
            leftPhoto: await fileToBase64(value.leftPhoto!),
            rightPhoto: await fileToBase64(value.rightPhoto!),
          }

          await updateFacePhotos.mutateAsync(photos)
        })
        form.reset()
        setFileInputVersion((version) => version + 1)
        setShowSuccess(true)
      } catch (error) {
        showError(error, t('error.picturesTitle'), t('error.picturesMessage'))
      }
    },
  })

  return (
    <form
      className="face-photo-form"
      onSubmit={(event) => {
        event.preventDefault()
        event.stopPropagation()
        void form.handleSubmit()
      }}
    >
      <div className="face-photo-form__header">
        <h2>{t('settings.facePictures')}</h2>
        <p>{t('settings.faceDescription')}</p>
        <ul className="face-photo-form__tips">
          <li>{t('settings.tipLighting')}</li>
          <li>{t('settings.tipAccessories')}</li>
          <li>{t('settings.tipFrame')}</li>
        </ul>
      </div>

      <div className="face-photo-form__fields">
        <form.Field
          name="leftPhoto"
          validators={{
            onChange: ({ value }) =>
              value ? undefined : t('validation.photoRequired'),
          }}
        >
          {(field) => (
            <FileInput
              key={fileInputVersion}
              label={t('settings.left')}
              accept="image/*"
              onChange={field.handleChange}
              error={
                field.state.meta.isTouched
                  ? field.state.meta.errors[0]
                  : undefined
              }
            />
          )}
        </form.Field>

        <form.Field
          name="straightPhoto"
          validators={{
            onChange: ({ value }) =>
              value ? undefined : t('validation.photoRequired'),
          }}
        >
          {(field) => (
            <FileInput
              key={fileInputVersion}
              label={t('settings.straight')}
              accept="image/*"
              onChange={field.handleChange}
              error={
                field.state.meta.isTouched
                  ? field.state.meta.errors[0]
                  : undefined
              }
            />
          )}
        </form.Field>

        <form.Field
          name="rightPhoto"
          validators={{
            onChange: ({ value }) =>
              value ? undefined : t('validation.photoRequired'),
          }}
        >
          {(field) => (
            <FileInput
              key={fileInputVersion}
              label={t('settings.right')}
              accept="image/*"
              onChange={field.handleChange}
              error={
                field.state.meta.isTouched
                  ? field.state.meta.errors[0]
                  : undefined
              }
            />
          )}
        </form.Field>
      </div>

      {showSuccess && (
        <p
          className="face-photo-form__success"
          role="status"
          aria-live="polite"
        >
          {t('settings.picturesUpdated')}
        </p>
      )}

      <Button
        type="submit"
        className="face-photo-form__submit"
        disabled={form.state.isSubmitting}
      >
        {t('settings.savePictures')}
      </Button>
    </form>
  )
}
