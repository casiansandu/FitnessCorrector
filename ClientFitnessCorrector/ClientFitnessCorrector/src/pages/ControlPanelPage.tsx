import { Link } from 'react-router-dom'

export function ControlPanelPage() {
  return (
    <div className="page-shell">
      <header className="hero">
        <div className="spark" aria-hidden="true" />
        <h1>Control Panel</h1>
        <p className="lede">Admin tools will live here. This page is ready for your future implementation.</p>
      </header>

      <section className="control-panel-card">
        <h2>Coming soon</h2>
        <p>Use this page for admin-only management features.</p>
        <Link className="control-panel-back" to="/workout">
          Back to Workout
        </Link>
      </section>
    </div>
  )
}
