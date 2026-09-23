import { useForm } from '@tanstack/react-form'
import { Link, useNavigate } from '@tanstack/react-router'
import { Button, ErrorModal, TextInput } from '../../../../components/ui'
import './LoginForm.scss'
import { validEmail, minPasswordLength } from '../../../../lib/validation'
import { useTranslation } from '../../../../lib/i18n'
import { useState } from 'react'
import { loginWithPassword } from '../../api/loginWithPassword'

export function LoginForm() {
  const navigate = useNavigate()
  const [loginError, setLoginError] = useState<string | null>(null)
  const { t } = useTranslation()

  const form = useForm({
    defaultValues: {
      email: '',
      password: '',
    },

    onSubmit: async ({ value }) => {
      try {
        await loginWithPassword(value.email, value.password)
        navigate({
          to: '/overview',
        })
      } catch (error) {
        setLoginError(
          error instanceof Error ? error.message : t('error.loginFailed'),
        )
      }
    },
  })

  function handleAdministratorLogin() {
    navigate({
      to: '/login-admin',
    })
  }

  if (loginError) {
    return (
      <ErrorModal
        isOpen={true}
        onClose={() => setLoginError(null)}
        title={t('error.loginFailedTitle')}
        message={loginError}
      />
    )
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
      <h1 className="login-form__title">{t('auth.loginStudent')}</h1>

      <div className="login-form__fields">
        <form.Field
          name="email"
          validators={{
            onChange: validEmail(t('validation.invalidEmail')),
          }}
        >
          {(field) => (
            <TextInput
              label={t('common.email')}
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
            onChange: minPasswordLength(
              8,
              t('validation.passwordLength', { length: 8 }),
            ),
          }}
        >
          {(field) => (
            <TextInput
              label={t('common.password')}
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
        <Link to="/reset-password">{t('auth.forgotPassword')}</Link>
      </div>

      <Button
        type="submit"
        className="login-form__submit"
        loading={form.state.isSubmitting}
      >
        {t('auth.login')}
      </Button>

      <div className="login-form__register">
        <Link to="/register">{t('auth.noAccount')}</Link>
      </div>

      <div className="login-form__administrator">
        <Button
          type="button"
          variant="secondary"
          onClick={handleAdministratorLogin}
        >
          {t('auth.loginAdministrator')}
        </Button>
      </div>
    </form>
  )
}
