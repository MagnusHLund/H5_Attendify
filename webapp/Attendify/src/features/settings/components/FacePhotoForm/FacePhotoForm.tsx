import { useForm } from '@tanstack/react-form'
import { Button, FileInput } from '../../../../components/ui'
import './FacePhotoForm.scss'

export function FacePhotoForm() {
  const form = useForm({
    defaultValues: {
      straightPhoto: null as File | null,
      leftPhoto: null as File | null,
      rightPhoto: null as File | null,
    },

    onSubmit: async ({ value }) => {
      console.log(value)

      // API call will go here.
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
              label="Straight angle"
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
              label="Slight left angle"
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
              label="Slight right angle"
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
        disabled={form.state.isSubmitting}
      >
        {form.state.isSubmitting ? 'Saving...' : 'Save pictures'}
      </Button>
    </form>
  )
}
