import './Spinner.scss'

interface SpinnerProps {
  label?: string
  size?: 'small' | 'medium' | 'large'
}

export function Spinner({
  label = 'Loading',
  size = 'medium',
}: SpinnerProps) {
  return (
    <span
      className={`spinner spinner--${size}`}
      role="status"
      aria-label={label}
    >
      <span className="spinner__ring spinner__ring--blue" aria-hidden="true" />
      <span className="spinner__ring spinner__ring--green" aria-hidden="true" />
    </span>
  )
}