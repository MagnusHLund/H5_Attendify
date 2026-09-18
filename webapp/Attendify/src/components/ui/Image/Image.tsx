import type { ImgHTMLAttributes } from 'react'

interface ImageProps
  extends Omit<ImgHTMLAttributes<HTMLImageElement>, 'alt'> {
  alt: string
}

export function Image({ alt, className, ...props }: ImageProps) {
  return <img alt={alt} className={className} {...props} />
}
