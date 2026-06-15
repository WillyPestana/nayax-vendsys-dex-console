import type { ReactNode } from 'react'

type AlertTone = 'danger' | 'success' | 'warning' | 'neutral'

type AlertProps = {
  tone?: AlertTone
  title?: string
  children: ReactNode
}

export function Alert({ tone = 'neutral', title, children }: AlertProps) {
  return (
    <div className={`ui-alert ui-alert-${tone}`} role={tone === 'danger' ? 'alert' : 'status'}>
      {title ? <strong>{title}</strong> : null}
      <div>{children}</div>
    </div>
  )
}
