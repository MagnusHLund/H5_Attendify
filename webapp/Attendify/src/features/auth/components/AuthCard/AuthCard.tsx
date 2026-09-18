import type { ReactNode } from 'react'
import { Image } from '../../../../components/ui'
import { useTranslation } from '../../../../lib/i18n'
import './AuthCard.scss'

interface AuthCardProps {
  children: ReactNode
}

export function AuthCard({ children }: AuthCardProps) {
  const { t } = useTranslation()

  return (
    <div className="auth-card">
      <Image
        src="/internal/logos/Attendify-large.png"
        alt={t('common.attendifyLogo')}
        className="auth-card__logo"
      />

      <div className="auth-card__content">{children}</div>
    </div>
  )
}
