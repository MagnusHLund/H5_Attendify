import { useForm } from '@tanstack/react-form'
import {
  Button,
  FileInput,
  useErrorModal,
} from '../../../../components/ui'
import { useUpdateFacePhotos } from '../../hooks/useUpdateFacePhotos'
import './FacePhotoForm.scss'

export function FacePhotoForm() {
  const updateFacePhotos = useUpdateFacePhotos()
  const { showError } = useErrorModal()

  const form = useForm({
    defaultValues: {
      straightPhoto: null as File | null,
      leftPhoto: null as File | null,
      rightPhoto: null as File | null,
    },

    onSubmit: async ({ value }) => {
      try {
        await updateFacePhotos.mutateAsync({
          straightPhoto: value.straightPhoto!,
          leftPhoto: value.leftPhoto!,
          rightPhoto: value.rightPhoto!,
        })
      } catch (error) {
        showError(error, 'Pictures could not be saved')
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
        <h2>Face pictures</h2>
        <p>
          Update the pictures used to identify you. Please make sure your face
          is clearly visible in each picture.
        </p>
        <ul className="face-photo-form__tips">
          <li>Use even lighting</li>
          <li>Remove hats and sunglasses</li>
          <li>Keep your whole face in frame</li>
        </ul>
      </div>

      <div className="face-photo-form__fields">
        <form.Field
          name="straightPhoto"
          validators={{
            onChange: ({ value }) =>
              value ? undefined : 'A photo is required',
          }}
        >
          {(field) => (
            <FileInput
              label="1. Looking straight ahead"
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
          name="leftPhoto"
          validators={{
            onChange: ({ value }) =>
              value ? undefined : 'A photo is required',
          }}
        >
          {(field) => (
            <FileInput
              label="2. Turned slightly left"
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
              value ? undefined : 'A photo is required',
          }}
        >
          {(field) => (
            <FileInput
              label="3. Turned slightly right"
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

      <Button
        type="submit"
        className="face-photo-form__submit"
        loading={form.state.isSubmitting}
      >
        Save pictures
      </Button>
    </form>
  )
}
