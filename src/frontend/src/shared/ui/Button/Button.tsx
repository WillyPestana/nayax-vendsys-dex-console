import type { ButtonHTMLAttributes, ElementType } from 'react'

type ButtonVariant = 'primary' | 'secondary' | 'ghost' | 'danger'

type ButtonProps = ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: ButtonVariant
  icon?: ElementType<{ size?: number; 'aria-hidden'?: boolean }>
  isLoading?: boolean
}

export function Button({
  variant = 'primary',
  icon: Icon,
  isLoading = false,
  children,
  className = '',
  disabled,
  ...props
}: ButtonProps) {
  return (
    <button
      className={`ui-button ui-button-${variant} ${className}`.trim()}
      disabled={disabled || isLoading}
      {...props}
    >
      {Icon ? <Icon size={18} aria-hidden={true} /> : null}
      <span>{isLoading ? 'Processing' : children}</span>
    </button>
  )
}
