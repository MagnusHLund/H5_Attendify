import type { ReactNode } from 'react'
import './AuthCard.scss'

interface AuthCardProps {
  children: ReactNode
}

export function AuthCard({ children }: AuthCardProps) {
  return (
    <div className="auth-card">
      <img
        src="/Attendify-large.png"
        alt="Attendify"
        className="auth-card__logo"
      />

      <div className="auth-card__content">{children}</div>
    </div>
  )
}
