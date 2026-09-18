import type { ButtonHTMLAttributes } from 'react'
import { Spinner } from '../Spinner/Spinner'
import './Button.scss'

type ButtonVariant = 'primary' | 'secondary' | 'danger' | 'ghost'

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant
  loading?: boolean
}

export function Button({
  variant = 'primary',
  loading = false,
  disabled,
  children,
  className,
  ...props
}: ButtonProps) {
  return (
    <button
      className={`button button--${variant} ${className ?? ''}`}
      disabled={disabled || loading}
      aria-busy={loading}
      {...props}
    >
      {loading && <Spinner size="small" />}
      {children}
    </button>
  )
}
