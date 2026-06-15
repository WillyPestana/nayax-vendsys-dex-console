import { zodResolver } from '@hookform/resolvers/zod'
import { LogIn } from 'lucide-react'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { useNavigate } from 'react-router-dom'
import { z } from 'zod'
import { useAuth } from '../../app/providers/AuthContext'
import { Alert } from '../../shared/ui/Alert/Alert'
import { Button } from '../../shared/ui/Button/Button'
import { TextField } from '../../shared/ui/TextField/TextField'

const loginSchema = z.object({
  username: z.string().min(1, 'Username is required.'),
  password: z.string().min(1, 'Password is required.'),
})

type LoginFormValues = z.infer<typeof loginSchema>

export function LoginPage() {
  const navigate = useNavigate()
  const { login } = useAuth()
  const [error, setError] = useState<string | null>(null)
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      username: 'vendsys',
      password: '',
    },
  })

  async function onSubmit(values: LoginFormValues) {
    setError(null)
    try {
      await login(values.username, values.password)
      navigate('/upload', { replace: true })
    } catch {
      setError('Invalid username or password.')
    }
  }

  return (
    <main className="auth-page">
      <section className="auth-panel">
        <div className="brand-block">
          <div className="brand-mark">NX</div>
          <div>
            <strong>Nayax VendSys</strong>
            <span>DEX Console</span>
          </div>
        </div>

        <div className="page-stack">
          <div>
            <h1>Sign in</h1>
            <p className="muted-copy">Validate your credentials before uploading DEX audit files.</p>
          </div>

          {error ? <Alert tone="danger">{error}</Alert> : null}

          <form className="form-stack" onSubmit={handleSubmit(onSubmit)}>
            <TextField label="Username" autoComplete="username" error={errors.username?.message} {...register('username')} />
            <TextField
              label="Password"
              type="password"
              autoComplete="current-password"
              error={errors.password?.message}
              {...register('password')}
            />
            <Button type="submit" icon={LogIn} isLoading={isSubmitting}>
              Sign in
            </Button>
          </form>
        </div>
      </section>

      <section className="auth-aside" aria-hidden="true">
        <h1>Machine audit data, parsed and ready for review.</h1>
      </section>
    </main>
  )
}
