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
  return (
    <div className="dropdown-field">
      {label && (
        <label htmlFor={id} className="dropdown-field__label">
          {label}
        </label>
      )}

      <select
        id={id}
        className={`dropdown-field__control ${
          error ? 'dropdown-field__control--error' : ''
        }`}
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

      {error && <span className="dropdown-field__error">{error}</span>}
    </div>
  )
}
