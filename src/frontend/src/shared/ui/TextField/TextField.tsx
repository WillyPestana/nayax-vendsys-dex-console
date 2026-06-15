import type { InputHTMLAttributes } from 'react'

type TextFieldProps = InputHTMLAttributes<HTMLInputElement> & {
  label: string
  error?: string
}

export function TextField({ label, error, id, ...props }: TextFieldProps) {
  const inputId = id ?? props.name

  return (
    <label className="text-field" htmlFor={inputId}>
      <span>{label}</span>
      <input id={inputId} aria-invalid={Boolean(error)} {...props} />
      {error ? <small role="alert">{error}</small> : null}
    </label>
  )
}
