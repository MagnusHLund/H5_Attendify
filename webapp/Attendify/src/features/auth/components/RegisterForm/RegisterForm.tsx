import { useState } from 'react'
import { useForm } from '@tanstack/react-form'
import { Link, useNavigate } from '@tanstack/react-router'
import {
  Button,
  Dropdown,
  FileInput,
  TextInput,
} from '../../../../components/ui'
import {
  minPasswordLength,
  required,
  validEmail,
} from '../../../../lib/validation'
import './RegisterForm.scss'

type RegistrationStep = 'details' | 'photos'

interface EducationalInstitute {
  id: string
  name: string
}

export function RegisterForm() {
  const [step, setStep] = useState<RegistrationStep>('details')

  // This will eventually come from the API.
  const educationalInstitutes: EducationalInstitute[] = []

  const form = useForm({
    defaultValues: {
      email: '',
      password: '',
      confirmPassword: '',
      educationalInstituteId: '',
      studentId: '',
      straightPhoto: null as File | null,
      leftPhoto: null as File | null,
      rightPhoto: null as File | null,
    },

    onSubmit: async ({ value }) => {
      // Registration logic will go here.
      console.log(value)
    },
  })

  function handleNext() {
    setStep('photos')
  }

  function handleBack() {
    setStep('details')
  }

  if (step === 'details') {
    return (
      <form
        className="register-form"
        onSubmit={(event) => {
          event.preventDefault()
          event.stopPropagation()
          handleNext()
        }}
      >
        <h1 className="register-form__title">Register new student</h1>

        <div className="register-form__fields">
          <form.Field
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
          </form.Field>

          <form.Field
            name="password"
            validators={{
              onChange: ({ value }) =>
                !value
                  ? 'Password is required'
                  : minPasswordLength(8)({ value }),
            }}
          >
            {(field) => (
              <TextInput
                label="Password"
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
          </form.Field>

          <form.Field
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
                label="Confirm password"
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
          </form.Field>

          <form.Field
            name="educationalInstituteId"
            validators={{
              onChange: required('Educational institute is required'),
            }}
          >
            {(field) => (
              <Dropdown
                label="Educational institute"
                name={field.name}
                value={field.state.value}
                options={educationalInstitutes.map((institute) => ({
                  value: institute.id,
                  label: institute.name,
                }))}
                placeholder="Select an educational institute"
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
            name="studentId"
            validators={{
              onChange: required('Student ID is required'),
            }}
          >
            {(field) => (
              <TextInput
                label="Student ID"
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

        <Button type="submit" className="register-form__submit">
          Next
        </Button>

        <div className="register-form__login">
          <Link to="/login">Already got a user? Login</Link>
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
        form.handleSubmit()
      }}
    >
      <h1 className="register-form__title">Register new student</h1>

      <div className="register-form__photos">
        <form.Field
          name="straightPhoto"
          validators={{
            onChange: ({ value }) =>
              value ? undefined : 'A photo is required',
          }}
        >
          {(field) => (
            <FileInput
              label="Take a photo of your face from a straight angle"
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
              label="Take a photo of your face from a slight left angle"
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
              label="Take a photo of your face from a slight right angle"
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
        className="register-form__submit"
        disabled={form.state.isSubmitting}
      >
        {form.state.isSubmitting ? 'Registering...' : 'Complete registration'}
      </Button>

      <div className="register-form__back">
        <Button type="button" variant="secondary" onClick={handleBack}>
          Back
        </Button>
      </div>
    </form>
  )
}
