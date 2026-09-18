import type { ReactNode } from 'react'
import { Image } from '../../../../components/ui'
import './AuthCard.scss'

interface AuthCardProps {
  children: ReactNode
}

export function AuthCard({ children }: AuthCardProps) {
  return (
    <div className="auth-card">
      <Image
        src="/Attendify-large.png"
        alt="Attendify"
        className="auth-card__logo"
      />

      <div className="auth-card__content">{children}</div>
    </div>
  )
}
