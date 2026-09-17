import { useId } from 'react'
import type { SelectHTMLAttributes } from 'react'
import './Dropdown.scss'

export interface DropdownOption {
  value: string
  label: string
}

interface DropdownProps extends Omit<
  SelectHTMLAttributes<HTMLSelectElement>,
  'children'
> {
  label?: string
  error?: string
  options: DropdownOption[]
  placeholder?: string
}

export function Dropdown({
  label,
  error,
  options,
  placeholder,
  id,
  ...props
}: DropdownProps) {
  const generatedId = useId()
  const dropdownId = id ?? generatedId
  const errorId = `${dropdownId}-error`

  return (
    <div className="dropdown-field">
      {label && (
        <label htmlFor={dropdownId} className="dropdown-field__label">
          {label}
        </label>
      )}

      <select
        id={dropdownId}
        className={`dropdown-field__control ${
          error ? 'dropdown-field__control--error' : ''
        }`}
        aria-invalid={error ? true : undefined}
        aria-describedby={error ? errorId : undefined}
        {...props}
      >
        {placeholder && (
          <option value="" disabled>
            {placeholder}
          </option>
        )}

        {options.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>

      {error && (
        <span id={errorId} className="dropdown-field__error">
          {error}
        </span>
      )}
    </div>
  )
}
