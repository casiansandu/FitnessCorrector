import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { registerUser } from '../api/auth'
import { hashPassword } from '../utils/passwordHash'

const isValidEmail = (value: string) => /\S+@\S+\.\S+/.test(value)
const isValidPassword = (value: string) =>
  value.length >= 8 && /[A-Z]/.test(value) && /[a-z]/.test(value) && /\d/.test(value)

export function RegisterPage() {
  const navigate = useNavigate()
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [statusMessage, setStatusMessage] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  const handleSubmit = async (event: Parameters<NonNullable<React.ComponentProps<'form'>['onSubmit']>>[0]) => {
    event.preventDefault()

    if (!firstName.trim() || !lastName.trim()) {
      setStatusMessage('Enter your first and last name.')
      return
    }

    if (!isValidEmail(email)) {
      setStatusMessage('Enter a valid email address.')
      return
    }
    if (!isValidPassword(password)) {
      setStatusMessage('Password must be at least 8 characters and include upper/lowercase letters and a number.')
      return
    }

    if (password !== confirmPassword) {
      setStatusMessage('Passwords do not match. Please re-enter them.')
      return
    }
    setIsSubmitting(true)
    setStatusMessage('Creating your account...')

    try {
      const passwordHash = await hashPassword(password)

      const registration = await registerUser({
        email: email.trim(),
        passwordHash,
        firstName: firstName.trim(),
        lastName: lastName.trim(),
      })

      localStorage.setItem('fitnessCorrectorUserId', registration.userId)

      setStatusMessage('Account created. Redirecting...')
      navigate('/workout')
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Registration failed. Try again.'
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
          <p className="eyebrow">Get started</p>
          <h1>Build your profile</h1>
          <p className="lede">
            Track your training sessions, compare form improvements, and share notes with your coach.
          </p>
        </section>

        <section className="auth-card" aria-label="Create account form">
          <header className="auth-header">
            <h2>Create account</h2>
            <p>Set up your profile in under a minute.</p>
          </header>

          <form className="auth-form" onSubmit={handleSubmit}>
            <label className="auth-field">
              <span>First name</span>
              <input
                type="text"
                placeholder="Jordan"
                autoComplete="given-name"
                value={firstName}
                onChange={(event) => setFirstName(event.target.value)}
              />
            </label>

            <label className="auth-field">
              <span>Last name</span>
              <input
                type="text"
                placeholder="Smith"
                autoComplete="family-name"
                value={lastName}
                onChange={(event) => setLastName(event.target.value)}
              />
            </label>

            <label className="auth-field">
              <span>Email address</span>
              <input
                type="email"
                placeholder="jordan@studio.com"
                autoComplete="email"
                value={email}
                onChange={(event) => setEmail(event.target.value)}
              />
            </label>

            <label className="auth-field">
              <span>Password</span>
              <input
                type="password"
                placeholder="Create a password"
                autoComplete="new-password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
              />
            </label>

            <label className="auth-field">
              <span>Confirm password</span>
              <input
                type="password"
                placeholder="Repeat your password"
                autoComplete="new-password"
                value={confirmPassword}
                onChange={(event) => setConfirmPassword(event.target.value)}
              />
            </label>

            <div className="auth-actions">
              <button type="submit" className="primary" disabled={isSubmitting}>
                {isSubmitting ? 'Creating account...' : 'Create account'}
              </button>
              <Link to="/login" className="ghost">
                Back to login
              </Link>
            </div>
            {statusMessage ? <p className="status-text">{statusMessage}</p> : null}
          </form>

          <footer className="auth-footer">
            <span>Already have an account?</span>
            <Link to="/login">Sign in</Link>
          </footer>
        </section>
      </div>
    </div>
  )
}
