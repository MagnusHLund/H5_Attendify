import { useForm } from '@tanstack/react-form'
import { useNavigate } from '@tanstack/react-router'
import { Button, TextInput } from '../../../../components/ui'
import { required } from '../../../../lib/validation'
import './LoginAdminForm.scss'

export function LoginAdminForm() {
  const navigate = useNavigate()

  const form = useForm({
    defaultValues: {
      accessCode: '',
    },

    onSubmit: async ({ value }) => {
      // Administrator login logic will go here.
      console.log(value)
    },
  })

  function handleStudentLogin() {
    navigate({
      to: '/login',
    })
  }

  return (
    <form
      className="login-admin-form"
      onSubmit={(event) => {
        event.preventDefault()
        event.stopPropagation()
        form.handleSubmit()
      }}
    >
      <h1 className="login-admin-form__title">Login as administrator</h1>

      <div className="login-admin-form__fields">
        <form.Field
          name="accessCode"
          validators={{
            onChange: required('Student access code is required'),
          }}
        >
          {(field) => (
            <TextInput
              label="Student access code"
              type="text"
              name={field.name}
              value={field.state.value}
              autoComplete="off"
              required
              onBlur={field.handleBlur}
              onChange={(event) => field.handleChange(event.target.value)}
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
        className="login-admin-form__submit"
        loading={form.state.isSubmitting}
      >
        Login
      </Button>

      <div className="login-admin-form__student">
        <Button type="button" variant="secondary" onClick={handleStudentLogin}>
          Login as student
        </Button>
      </div>
    </form>
  )
}
