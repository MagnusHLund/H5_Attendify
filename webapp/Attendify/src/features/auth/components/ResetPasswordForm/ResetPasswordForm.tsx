import { useState } from 'react'
import { useForm } from '@tanstack/react-form'
import { useNavigate } from '@tanstack/react-router'
import { Button, TextInput } from '../../../../components/ui'
import {
  required,
  validEmail,
  minPasswordLength,
} from '../../../../lib/validation'
import './ResetPasswordForm.scss'

type ResetPasswordStep = 'email' | 'code' | 'password'

export function ResetPasswordForm() {
  const navigate = useNavigate()
  const [step, setStep] = useState<ResetPasswordStep>('email')

  const emailForm = useForm({
    defaultValues: {
      email: '',
    },

    // Send recovery code to the email address.
    onSubmit: async ({ value }) => {
      // TODO: Call endpoint

      setStep('code')
    },
  })

  const codeForm = useForm({
    defaultValues: {
      code: '',
    },

    // Verify the recovery code.
    onSubmit: async ({ value }) => {
      // TODO: Call endpoint

      setStep('password')
    },
  })

  const passwordForm = useForm({
    defaultValues: {
      password: '',
      confirmPassword: '',
    },

    // Save the new password.
    onSubmit: async ({ value }) => {
      // TODO: Call endpoint

      // TODO If successful then navigate to the login page. Otherwise modal with error message.
      navigate({
        to: '/login',
      })
    },
  })

  function handleBack() {
    navigate({
      to: '/login',
    })
  }

  function handleResendCode() {
    // TODO: Resend recovery code to `email`.
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
        <h1 className="reset-password-form__title">Forgot password</h1>

        <div className="reset-password-form__fields">
          <emailForm.Field
            name="email"
            validators={{
              onChange: ({ value }) =>
                !value.trim()
                  ? required('Email is required')({ value })
                  : validEmail()({ value }),
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
          </emailForm.Field>
        </div>

        <Button
          type="submit"
          className="reset-password-form__submit"
          disabled={emailForm.state.isSubmitting}
        >
          {emailForm.state.isSubmitting ? 'Sending...' : 'Send code'}
        </Button>

        <div className="reset-password-form__back">
          <Button type="button" variant="secondary" onClick={handleBack}>
            Back
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
        <h1 className="reset-password-form__title">Enter recovery code</h1>

        <div className="reset-password-form__fields">
          <codeForm.Field
            name="code"
            validators={{
              onChange: required('Recovery code is required'),
            }}
          >
            {(field) => (
              <TextInput
                label="Code"
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
          {codeForm.state.isSubmitting ? 'Submitting...' : 'Submit'}
        </Button>

        <div className="reset-password-form__resend">
          <button type="button" onClick={handleResendCode}>
            Resend code
          </button>
        </div>

        <div className="reset-password-form__back">
          <Button type="button" variant="secondary" onClick={handleBack}>
            Back
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
      <h1 className="reset-password-form__title">Enter new password</h1>

      <div className="reset-password-form__fields">
        <passwordForm.Field
          name="password"
          validators={{
            onChange: minPasswordLength(8),
          }}
        >
          {(field) => (
            <TextInput
              label="New password"
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
                ? 'Passwords do not match'
                : undefined,
          }}
        >
          {(field) => (
            <TextInput
              label="Re-enter new password"
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
        {passwordForm.state.isSubmitting ? 'Saving...' : 'Save new password'}
      </Button>

      <div className="reset-password-form__back">
        <Button type="button" variant="secondary" onClick={handleBack}>
          Back
        </Button>
      </div>
    </form>
  )
}
