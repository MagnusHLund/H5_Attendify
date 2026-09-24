import { useEffect, useState } from 'react'
import { useForm } from '@tanstack/react-form'
import { useNavigate } from '@tanstack/react-router'
import {
  Button,
  TextInput,
  useErrorModal,
  useLoadingOverlay,
} from '../../../../components/ui'
import {
  required,
  validEmail,
  minPasswordLength,
} from '../../../../lib/validation'
import { useTranslation } from '../../../../lib/i18n'
import './ResetPasswordForm.scss'
import {
  requestResetPassword,
  verifyResetPassword,
  completeResetPassword,
} from '../../api/resetPassword'

type ResetPasswordStep = 'email' | 'code' | 'password'
const RESEND_COOLDOWN_SECONDS = 30

export function ResetPasswordForm() {
  const navigate = useNavigate()
  const { t } = useTranslation()
  const { showError } = useErrorModal()
  const { runWithLoading } = useLoadingOverlay()
  const [email, setEmail] = useState<string>('')
  const [securityCode, setSecurityCode] = useState<string>('')
  const [step, setStep] = useState<ResetPasswordStep>('email')
  const [resendCooldown, setResendCooldown] = useState(0)
  const [isResending, setIsResending] = useState(false)

  useEffect(() => {
    if (resendCooldown <= 0) return

    const timeoutId = window.setTimeout(() => {
      setResendCooldown((remaining) => Math.max(remaining - 1, 0))
    }, 1000)

    return () => window.clearTimeout(timeoutId)
  }, [resendCooldown])

  const emailForm = useForm({
    defaultValues: {
      email: '',
    },

    // Send recovery code to the email address.
    onSubmit: async ({ value }) => {
      try {
        setEmail(value.email)
        await runWithLoading(() => requestResetPassword(value.email))
        setResendCooldown(RESEND_COOLDOWN_SECONDS)
        setStep('code')
      } catch (err) {
        showError(err, t('error.resetPasswordErrorTitle'))
      }
    },
  })

  const codeForm = useForm({
    defaultValues: {
      code: '',
    },

    // Verify the recovery code.
    onSubmit: async ({ value }) => {
      try {
        setSecurityCode(value.code)
        await runWithLoading(() => verifyResetPassword(email, value.code))
        setStep('password')
      } catch (err) {
        showError(err, t('error.resetPasswordErrorTitle'))
      }
    },
  })

  const passwordForm = useForm({
    defaultValues: {
      password: '',
      confirmPassword: '',
    },

    // Save the new password.
    onSubmit: async ({ value }) => {
      try {
        if (value.password !== value.confirmPassword) {
          throw new Error('Passwords do not match')
        }

        await runWithLoading(() =>
          completeResetPassword(email, securityCode, value.password),
        )

        navigate({
          to: '/login',
        })
      } catch (err) {
        showError(err, t('error.resetPasswordErrorTitle'))
        return
      }
    },
  })

  function handleBack() {
    navigate({
      to: '/login',
    })
  }

  async function handleResendCode() {
    if (isResending || resendCooldown > 0) return

    setIsResending(true)

    try {
      await runWithLoading(() => requestResetPassword(email))
      setResendCooldown(RESEND_COOLDOWN_SECONDS)
    } catch (err) {
      showError(err, t('error.resetPasswordErrorTitle'))
    } finally {
      setIsResending(false)
    }
  }

  if (step === 'email') {
    return (
      <form
        className="reset-password-form"
        onSubmit={(event) => {
          event.preventDefault()
          event.stopPropagation()
          emailForm.handleSubmit()
        }}
      >
        <h1 className="reset-password-form__title">{t('reset.forgotTitle')}</h1>

        <div className="reset-password-form__fields">
          <emailForm.Field
            name="email"
            validators={{
              onChange: ({ value }) =>
                !value.trim()
                  ? required(t('validation.emailRequired'))({ value })
                  : validEmail(t('validation.invalidEmail'))({ value }),
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
          </emailForm.Field>
        </div>

        <Button
          type="submit"
          className="reset-password-form__submit"
          disabled={emailForm.state.isSubmitting}
        >
          {t('reset.sendCode')}
        </Button>

        <div className="reset-password-form__back">
          <Button type="button" variant="secondary" onClick={handleBack}>
            {t('common.back')}
          </Button>
        </div>
      </form>
    )
  }

  if (step === 'code') {
    return (
      <form
        className="reset-password-form"
        onSubmit={(event) => {
          event.preventDefault()
          event.stopPropagation()
          codeForm.handleSubmit()
        }}
      >
        <h1 className="reset-password-form__title">{t('reset.codeTitle')}</h1>

        <div className="reset-password-form__fields">
          <codeForm.Field
            name="code"
            validators={{
              onChange: required(t('validation.recoveryCodeRequired')),
            }}
          >
            {(field) => (
              <TextInput
                label={t('reset.code')}
                type="text"
                name={field.name}
                value={field.state.value}
                autoComplete="one-time-code"
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
          </codeForm.Field>
        </div>

        <Button
          type="submit"
          className="reset-password-form__submit"
          disabled={codeForm.state.isSubmitting}
        >
          {t('reset.submit')}
        </Button>

        <div className="reset-password-form__resend">
          <button
            type="button"
            onClick={() => void handleResendCode()}
            disabled={resendCooldown > 0 || isResending}
          >
            {isResending
              ? t('reset.resendingCode')
              : resendCooldown > 0
                ? t('reset.resendCodeCooldown', { seconds: resendCooldown })
                : t('reset.resendCode')}
          </button>
        </div>

        <div className="reset-password-form__back">
          <Button type="button" variant="secondary" onClick={handleBack}>
            {t('common.back')}
          </Button>
        </div>
      </form>
    )
  }

  return (
    <form
      className="reset-password-form"
      onSubmit={(event) => {
        event.preventDefault()
        event.stopPropagation()
        passwordForm.handleSubmit()
      }}
    >
      <h1 className="reset-password-form__title">{t('reset.passwordTitle')}</h1>

      <div className="reset-password-form__fields">
        <passwordForm.Field
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
              label={t('reset.newPassword')}
              type="password"
              name={field.name}
              value={field.state.value}
              autoComplete="new-password"
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
        </passwordForm.Field>

        <passwordForm.Field
          name="confirmPassword"
          validators={{
            onChangeListenTo: ['password'],
            onChange: ({ value, fieldApi }) =>
              value !== fieldApi.form.getFieldValue('password')
                ? t('validation.passwordMismatch')
                : undefined,
          }}
        >
          {(field) => (
            <TextInput
              label={t('reset.repeatPassword')}
              type="password"
              name={field.name}
              value={field.state.value}
              autoComplete="new-password"
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
        </passwordForm.Field>
      </div>

      <Button
        type="submit"
        className="reset-password-form__submit"
        disabled={passwordForm.state.isSubmitting}
      >
        {t('reset.savePassword')}
      </Button>

      <div className="reset-password-form__back">
        <Button type="button" variant="secondary" onClick={handleBack}>
          {t('common.back')}
        </Button>
      </div>
    </form>
  )
}
