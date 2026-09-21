import './Spinner.scss'
import { useTranslation } from '../../../lib/i18n'

interface SpinnerProps {
  label?: string
  size?: 'small' | 'medium' | 'large'
}

export function Spinner({
  className,
  label,
  size = 'medium',
}: SpinnerProps & { className?: string }) {
  const { t } = useTranslation()

  return (
    <span
      className={`spinner spinner--${size} ${className ?? ''}`}
      role="status"
      aria-label={label ?? t('common.loading')}
    >
      <span className="spinner__ring spinner__ring--blue" aria-hidden="true" />
      <span className="spinner__ring spinner__ring--green" aria-hidden="true" />
    </span>
  )
}
