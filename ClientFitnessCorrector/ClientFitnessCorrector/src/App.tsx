import './App.css'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { ControlPanelPage } from './pages/ControlPanelPage'
import { LoginPage } from './pages/LoginPage'
import { RegisterPage } from './pages/RegisterPage'
import { WorkoutPage } from './pages/WorkoutPage'
import { SubscriptionsPage } from './pages/SubscriptionsPage'
import { SubscriptionSuccessPage } from './pages/SubscriptionSuccessPage'
import { ProgressPage } from './pages/ProgressPage'

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Navigate to="/login" replace />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/workout" element={<WorkoutPage />} />
        <Route path="/progress" element={<ProgressPage />} />
        <Route path="/control-panel" element={<ControlPanelPage />} />
        <Route path="/subscriptions" element={<SubscriptionsPage />} />
        <Route path="/success" element={<SubscriptionSuccessPage />} />
      </Routes>
    </BrowserRouter>
  )
}

export default App
