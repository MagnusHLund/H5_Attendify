import './Spinner.scss'
import { useTranslation } from '../../../lib/i18n'

interface SpinnerProps {
  decorative?: boolean
  label?: string
  size?: 'small' | 'medium' | 'large'
}

export function Spinner({
  className,
  decorative = false,
  label,
  size = 'medium',
}: SpinnerProps & { className?: string }) {
  const { t } = useTranslation()

  return (
    <span
      className={`spinner spinner--${size} ${className ?? ''}`}
      role={decorative ? undefined : 'status'}
      aria-label={decorative ? undefined : (label ?? t('common.loading'))}
      aria-hidden={decorative || undefined}
    >
      <span className="spinner__ring spinner__ring--blue" aria-hidden="true" />
      <span className="spinner__ring spinner__ring--green" aria-hidden="true" />
    </span>
  )
}
