import type { HTMLAttributes } from 'react'
import './PageContainer.scss'

interface PageContainerProps extends HTMLAttributes<HTMLDivElement> {
  children: React.ReactNode
}

export function PageContainer({
  children,
  className = '',
  ...props
}: PageContainerProps) {
  return (
    <div
      className={`page-container ${className}`.trim()}
      {...props}
    >
      {children}
    </div>
  )
}
