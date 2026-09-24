import { useEffect, useRef, useState } from 'react'
import { useForm } from '@tanstack/react-form'
import { Link, useNavigate } from '@tanstack/react-router'
import {
  Button,
  Dropdown,
  FileInput,
  Spinner,
  TextInput,
  useErrorModal,
  useLoadingOverlay,
} from '../../../../components/ui'
import { useEducationalInstitutes } from '../../../../features/educationalInstitutes/hooks/useEducationalInstitutes'
import { fileToBase64 } from '../../../../lib/encoding/base64'
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
  const { showError } = useErrorModal()
  const { runWithLoading } = useLoadingOverlay()
  const navigate = useNavigate()
  const {
    data: educationalInstitutes,
    isPending,
    isError,
  } = useEducationalInstitutes()
  const { t } = useTranslation()

  const institutesErrorShown = useRef(false)

  const institutesUnavailable =
    !isPending &&
    (isError || !educationalInstitutes || educationalInstitutes.length === 0)

  useEffect(() => {
    if (!institutesUnavailable) {
      institutesErrorShown.current = false
      return
    }

    if (institutesErrorShown.current) return

    institutesErrorShown.current = true
    showError(
      new Error(t('error.educationalInstitutesNotFound')),
      t('error.educationalInstitutesNotFoundTitle'),
      t('error.educationalInstitutesNotFound'),
    )
  }, [institutesUnavailable, showError, t])

  const detailsForm = useForm({
    defaultValues: {
      email: '',
      password: '',
      confirmPassword: '',
      educationalInstituteId: '',
      studentId: '',
    },

    onSubmit: async () => {
      // The details are valid, so move to the photo step.
      setStep('photos')
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
        await runWithLoading(async () => {
          const registration = {
            email: detailsForm.state.values.email,
            password: detailsForm.state.values.password,
            educationalInstituteId:
              detailsForm.state.values.educationalInstituteId,
            studentId: detailsForm.state.values.studentId,
            straightPhoto: await fileToBase64(value.straightPhoto),
            leftPhoto: await fileToBase64(value.leftPhoto),
            rightPhoto: await fileToBase64(value.rightPhoto),
          }

          await registerStudent(registration)
        })

        await navigate({ to: '/overview' })
      } catch (error) {
        const message =
          error instanceof Error ? error.message : t('error.registrationFailed')

        showError(
          new Error(message),
          t('error.registrationFailedTitle'),
          t('error.registrationFailed'),
        )
      }
    },
  })

  function handleBack() {
    setStep('details')
  }

  if (isPending) {
    return <Spinner className="register-form__spinner" />
  }

  if (institutesUnavailable) {
    return null
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
        disabled={detailsForm.state.isSubmitting}
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
        disabled={photosForm.state.isSubmitting}
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
