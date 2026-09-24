import { useForm } from '@tanstack/react-form'
import { useNavigate } from '@tanstack/react-router'

import { Button, TextInput, useErrorModal } from '../../../../components/ui'
import { loginWithStudentAccessCode } from '../../api/loginWithStudentAccessCode'
import { required } from '../../../../lib/validation'
import { useTranslation } from '../../../../lib/i18n'
import './LoginAdminForm.scss'

export function LoginAdminForm() {
  const navigate = useNavigate()
  const { showError } = useErrorModal()
  const { t } = useTranslation()

  const form = useForm({
    defaultValues: {
      accessCode: '',
    },

    onSubmit: async ({ value }) => {
      try {
        await loginWithStudentAccessCode(value.accessCode)
        navigate({
          to: '/overview',
        })
      } catch (error) {
        showError(
          error,
          t('error.loginFailedTitle'),
          t('error.loginFailedAccessCode'),
        )
      }
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
      <h1 className="login-admin-form__title">
        {t('auth.loginAdministrator')}
      </h1>

      <div className="login-admin-form__fields">
        <form.Field
          name="accessCode"
          validators={{
            onChange: required(t('validation.accessCodeRequired')),
          }}
        >
          {(field) => (
            <TextInput
              label={t('auth.studentAccessCode')}
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
        {t('auth.login')}
      </Button>

      <div className="login-admin-form__student">
        <Button type="button" variant="secondary" onClick={handleStudentLogin}>
          {t('auth.loginStudent')}
        </Button>
      </div>
    </form>
  )
}
