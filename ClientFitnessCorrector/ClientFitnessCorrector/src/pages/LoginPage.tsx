import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { loginUser } from '../api/auth'
import { hashPassword } from '../utils/passwordHash'

const USER_ID_STORAGE_KEY = 'fitnessCorrectorUserId'
const IS_ADMIN_STORAGE_KEY = 'fitnessCorrectorIsAdmin'

const isValidEmail = (value: string) => /\S+@\S+\.\S+/.test(value)
const isValidPassword = (value: string) =>
  value.length >= 8 && /[A-Z]/.test(value) && /[a-z]/.test(value) && /\d/.test(value)

export function LoginPage() {
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [statusMessage, setStatusMessage] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  const handleSubmit = async (event: Parameters<NonNullable<React.ComponentProps<'form'>['onSubmit']>>[0]) => {
    event.preventDefault()

    if (!isValidEmail(email)) {
      setStatusMessage('Enter a valid email address.')
      return
    }

    if (!isValidPassword(password)) {
      setStatusMessage('Password must be at least 8 characters and include upper/lowercase letters and a number.')
      return
    }

    setIsSubmitting(true)
    setStatusMessage('Signing you in...')

    try {
      const passwordHash = await hashPassword(password)

      const loginResult = await loginUser({
        email: email.trim(),
        passwordHash,
      })

      localStorage.setItem(USER_ID_STORAGE_KEY, loginResult.userId)

      if (typeof loginResult.role === 'string') {
        localStorage.setItem(IS_ADMIN_STORAGE_KEY, String(loginResult.role.toLowerCase() === 'admin'))
      } else {
        localStorage.removeItem(IS_ADMIN_STORAGE_KEY)
      }

      setStatusMessage('Signed in. Redirecting...')
      navigate('/workout')
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Sign in failed. Try again.'
      setStatusMessage(message)
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="auth-shell">
      <div className="auth-grid">
        <section className="auth-panel">
          <div className="spark" aria-hidden="true" />
          <p className="eyebrow">Welcome back</p>
          <h1>Fitness Corrector</h1>
          <p className="lede">
            Get precision form feedback, session by session. Log in to keep your workout analysis history in one place.
          </p>
        </section>

        <section className="auth-card" aria-label="Login form">
          <header className="auth-header">
            <h2>Sign in</h2>
            <p>Use your email and password to continue.</p>
          </header>

          <form className="auth-form" onSubmit={handleSubmit}>
            <label className="auth-field">
              <span>Email address</span>
              <input
                type="email"
                placeholder="coach@studio.com"
                autoComplete="email"
                value={email}
                onChange={(event) => setEmail(event.target.value)}
              />
            </label>

            <label className="auth-field">
              <span>Password</span>
              <input
                type="password"
                placeholder="••••••••"
                autoComplete="current-password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
              />
            </label>

            <div className="auth-actions">
              <button type="submit" className="primary" disabled={isSubmitting}>
                {isSubmitting ? 'Signing in...' : 'Sign in'}
              </button>
            </div>
            {statusMessage ? <p className="status-text">{statusMessage}</p> : null}
          </form>

          <footer className="auth-footer">
            <span>New here?</span>
            <Link to="/register">Create an account</Link>
          </footer>
        </section>
      </div>
    </div>
  )
}
