import { Link, useNavigate } from 'react-router-dom'

export function SubscriptionSuccessPage() {
  const navigate = useNavigate()

  return (
    <div className="auth-shell">
      <div className="auth-grid">
        <section className="auth-panel">
          <div className="spark" aria-hidden="true" />
          <p className="eyebrow">Payment completed</p>
          <h1>Abonamentul este activ</h1>
          <p className="lede">
            Plata a fost procesata cu succes, iar accesul tau la analiza completa este activ.
          </p>
        </section>

        <section className="auth-card" aria-label="Subscription success">
          <header className="auth-header">
            <h2>Totul este in regula</h2>
            <p>Poti reveni in aplicatie si continua antrenamentele.</p>
          </header>

          <div className="auth-actions">
            <button type="button" className="primary" onClick={() => navigate('/workout')}>
              Inapoi la Home
            </button>
            <Link className="ghost" to="/subscriptions">
              Vezi abonamentul
            </Link>
          </div>
        </section>
      </div>
    </div>
  )
}
