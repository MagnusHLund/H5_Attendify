import { useForm } from '@tanstack/react-form'
import { Link, useNavigate } from '@tanstack/react-router'
import { Button, TextInput } from '../../../../components/ui'
import './LoginForm.scss'
import { validEmail, minPasswordLength } from '../../../../lib/validation'

export function LoginForm() {
  const navigate = useNavigate()

  const form = useForm({
    defaultValues: {
      email: '',
      password: '',
    },

    onSubmit: async ({ value }) => {
      // Login logic will go here.
      console.log(value)
    },
  })

  function handleAdministratorLogin() {
    navigate({
      to: '/login-admin',
    })
  }

  return (
    <form
      className="login-form"
      onSubmit={(event) => {
        event.preventDefault()
        event.stopPropagation()
        form.handleSubmit()
      }}
    >
      <h1 className="login-form__title">Login as student</h1>

      <div className="login-form__fields">
        <form.Field
          name="email"
          validators={{
            onChange: validEmail(),
          }}
        >
          {(field) => (
            <TextInput
              label="Email"
              type="email"
              name={field.name}
              value={field.state.value}
              autoComplete="email"
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

        <form.Field
          name="password"
          validators={{
            onChange: minPasswordLength(8),
          }}
        >
          {(field) => (
            <TextInput
              label="Password"
              type="password"
              name={field.name}
              value={field.state.value}
              autoComplete="current-password"
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

      <div className="login-form__reset-password">
        <Link to="/reset-password">Forgot password?</Link>
      </div>

      <Button
        type="submit"
        className="login-form__submit"
        loading={form.state.isSubmitting}
      >
        Login
      </Button>

      <div className="login-form__register">
        <Link to="/register">Not a user yet? Sign up</Link>
      </div>

      <div className="login-form__administrator">
        <Button
          type="button"
          variant="secondary"
          onClick={handleAdministratorLogin}
        >
          Login as administrator
        </Button>
      </div>
    </form>
  )
}
