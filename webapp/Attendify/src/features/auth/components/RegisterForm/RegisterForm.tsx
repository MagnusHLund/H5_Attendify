import { useState } from 'react'
import { useForm } from '@tanstack/react-form'
import { Link, useNavigate } from '@tanstack/react-router'
import { useEducationalInstitutes } from '../../../../features/educationalInstitutes/hooks/useEducationalInstitutes'
import {
  Button,
  Dropdown,
  ErrorModal,
  FileInput,
  Spinner,
  TextInput,
} from '../../../../components/ui'
import {
  minPasswordLength,
  required,
  validEmail,
} from '../../../../lib/validation'
import { useTranslation } from '../../../../lib/i18n'
import { registerStudent } from '../../api/registerStudent'
import './RegisterForm.scss'

type RegistrationStep = 'details' | 'photos'

export function RegisterForm() {
  const [step, setStep] = useState<RegistrationStep>('details')
  const [registrationError, setRegistrationError] = useState<string | null>(
    null,
  )
  const navigate = useNavigate()
  const {
    data: educationalInstitutes,
    isPending,
    isError,
  } = useEducationalInstitutes()
  const { t } = useTranslation()

  const detailsForm = useForm({
    defaultValues: {
      email: '',
      password: '',
      confirmPassword: '',
      educationalInstituteId: '',
      studentId: '',
    },

    onSubmit: async ({ value }) => {
      // The details are valid, so move to the photo step.
      setStep('photos')

      // The values remain available in detailsForm while this component
      // is mounted and can be used when the registration is completed.
      console.log(value)
    },
  })

  const photosForm = useForm({
    defaultValues: {
      straightPhoto: null as File | null,
      leftPhoto: null as File | null,
      rightPhoto: null as File | null,
    },

    onSubmit: async ({ value }) => {
      if (!value.straightPhoto || !value.leftPhoto || !value.rightPhoto) {
        return
      }

      try {
        setRegistrationError(null)

        await registerStudent({
          email: detailsForm.state.values.email,
          password: detailsForm.state.values.password,
          educationalInstituteId:
            detailsForm.state.values.educationalInstituteId,
          studentId: detailsForm.state.values.studentId,
          straightPhoto: value.straightPhoto,
          leftPhoto: value.leftPhoto,
          rightPhoto: value.rightPhoto,
        })
        await navigate({ to: '/login' })
      } catch (error) {
        setRegistrationError(
          error instanceof Error
            ? error.message
            : t('error.registrationFailed'),
        )
      }
    },
  })

  function handleBack() {
    setStep('details')
  }

  if (registrationError) {
    return (
      <ErrorModal
        title={t('error.registrationFailedTitle')}
        message={registrationError}
        isOpen={true}
        onClose={() => setRegistrationError(null)}
      />
    )
  }

  if (true) {
    return <Spinner className="register-form__spinner" />
  }

  if (isError || educationalInstitutes?.length === 0) {
    return (
      <ErrorModal
        title={t('error.educationalInstitutesNotFoundTitle')}
        message={t('error.educationalInstitutesNotFound')}
        isOpen={true}
        onClose={() => {
          navigate({ to: '/login' })
        }}
      />
    )
  }

  if (!educationalInstitutes) {
    return (
      <ErrorModal
        title={t('error.educationalInstitutesNotFoundTitle')}
        message={t('error.educationalInstitutesNotFound')}
        isOpen={true}
        onClose={() => {
          navigate({ to: '/login' })
        }}
      />
    )
  }

  if (step === 'details') {
    return (
      <form
        className="register-form"
        onSubmit={(event) => {
          event.preventDefault()
          event.stopPropagation()
          void detailsForm.handleSubmit()
        }}
      >
        <h1 className="register-form__title">{t('auth.registerTitle')}</h1>

        <div className="register-form__fields">
          <detailsForm.Field
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
          </detailsForm.Field>

          <detailsForm.Field
            name="password"
            validators={{
              onChange: ({ value }) =>
                !value
                  ? t('validation.passwordRequired')
                  : minPasswordLength(
                      8,
                      t('validation.passwordLength', { length: 8 }),
                    )({ value }),
            }}
          >
            {(field) => (
              <TextInput
                label={t('common.password')}
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
          </detailsForm.Field>

          <detailsForm.Field
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
                label={t('auth.confirmPassword')}
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
          </detailsForm.Field>

          <detailsForm.Field
            name="educationalInstituteId"
            validators={{
              onChange: required(t('validation.instituteRequired')),
            }}
          >
            {(field) => (
              <Dropdown
                label={t('auth.educationalInstitute')}
                name={field.name}
                value={field.state.value}
                options={educationalInstitutes.map((institute) => ({
                  value: institute.id,
                  label: institute.name,
                }))}
                placeholder={t('auth.selectEducationalInstitute')}
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
          </detailsForm.Field>

          <detailsForm.Field
            name="studentId"
            validators={{
              onChange: required(t('validation.studentIdRequired')),
            }}
          >
            {(field) => (
              <TextInput
                label={t('auth.studentId')}
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
          </detailsForm.Field>
        </div>

        <Button
          type="submit"
          className="register-form__submit"
          loading={detailsForm.state.isSubmitting}
        >
          {t('common.next')}
        </Button>

        <div className="register-form__login">
          <Link to="/login">{t('auth.alreadyUser')}</Link>
        </div>
      </form>
    )
  }

  return (
    <form
      className="register-form"
      onSubmit={(event) => {
        event.preventDefault()
        event.stopPropagation()
        void photosForm.handleSubmit()
      }}
    >
      <h1 className="register-form__title">{t('auth.registerTitle')}</h1>

      <div className="register-form__photos">
        <photosForm.Field
          name="straightPhoto"
          validators={{
            onChange: ({ value }) =>
              value ? undefined : t('validation.photoRequired'),
          }}
        >
          {(field) => (
            <FileInput
              label={t('auth.photoStraight')}
              accept="image/*"
              onChange={field.handleChange}
              error={
                field.state.meta.isTouched
                  ? field.state.meta.errors[0]
                  : undefined
              }
            />
          )}
        </photosForm.Field>

        <photosForm.Field
          name="leftPhoto"
          validators={{
            onChange: ({ value }) =>
              value ? undefined : t('validation.photoRequired'),
          }}
        >
          {(field) => (
            <FileInput
              label={t('auth.photoLeft')}
              accept="image/*"
              onChange={field.handleChange}
              error={
                field.state.meta.isTouched
                  ? field.state.meta.errors[0]
                  : undefined
              }
            />
          )}
        </photosForm.Field>

        <photosForm.Field
          name="rightPhoto"
          validators={{
            onChange: ({ value }) =>
              value ? undefined : t('validation.photoRequired'),
          }}
        >
          {(field) => (
            <FileInput
              label={t('auth.photoRight')}
              accept="image/*"
              onChange={field.handleChange}
              error={
                field.state.meta.isTouched
                  ? field.state.meta.errors[0]
                  : undefined
              }
            />
          )}
        </photosForm.Field>
      </div>

      <Button
        type="submit"
        className="register-form__submit"
        loading={photosForm.state.isSubmitting}
      >
        {t('auth.completeRegistration')}
      </Button>

      <div className="register-form__back">
        <Button type="button" variant="secondary" onClick={handleBack}>
          {t('common.back')}
        </Button>
      </div>
    </form>
  )
}
