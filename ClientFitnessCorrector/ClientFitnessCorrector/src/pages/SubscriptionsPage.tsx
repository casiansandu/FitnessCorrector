import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { PlanType, SubscriptionStatus, type Subscription } from '../types/subscription'
import { subscriptionApi } from '../api/subscriptions'
import { PricingPlans } from '../components/PricingPlans'
import { SubscriptionManagement } from '../components/SubscriptionManagement'

const PENDING_CHECKOUT_KEY = 'fitnessCorrectorPendingCheckout'

export function SubscriptionsPage() {
  const navigate = useNavigate()
  const [subscription, setSubscription] = useState<Subscription | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [creatingSubscription, setCreatingSubscription] = useState(false)

  useEffect(() => {
    loadSubscription()
  }, [])

  const loadSubscription = async () => {
    try {
      setLoading(true)
      const data = await subscriptionApi.getMySubscription()
      setSubscription(data)

      const pendingCheckout = sessionStorage.getItem(PENDING_CHECKOUT_KEY) === 'true'
      if (pendingCheckout && data?.status === SubscriptionStatus.Active) {
        sessionStorage.removeItem(PENDING_CHECKOUT_KEY)
        navigate('/success', { replace: true })
      }
    } catch (err) {
      setError((err as Error).message || 'Failed to load subscription')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    const pendingCheckout = sessionStorage.getItem(PENDING_CHECKOUT_KEY) === 'true'
    if (!pendingCheckout) {
      return
    }

    const timer = globalThis.setInterval(() => {
      void loadSubscription()
    }, 3000)

    return () => {
      globalThis.clearInterval(timer)
    }
  }, [])

  const handleSelectPlan = async (planType: PlanType) => {
    try {
      setCreatingSubscription(true)
      setError(null)
      
      const createdSubscription = await subscriptionApi.createSubscription({ planType })

      if (createdSubscription.checkoutUrl) {
        sessionStorage.setItem(PENDING_CHECKOUT_KEY, 'true')
        globalThis.location.assign(createdSubscription.checkoutUrl)
        return
      }
      
      // Reload subscription
      await loadSubscription()
      alert('Subscription created successfully!')
    } catch (err) {
      setError((err as Error).message || 'Failed to create subscription')
    } finally {
      setCreatingSubscription(false)
    }
  }

  if (loading) {
    return (
      <div className="page-shell">
        <header className="hero">
          <div className="spark" aria-hidden="true" />
          <h1>Subscription</h1>
          <p className="lede">Se incarca datele abonamentului tau...</p>
        </header>
      </div>
    )
  }

  return (
    <div className="page-shell">
      <div className="top-actions">
        <button type="button" className="logout-chip" onClick={() => navigate('/workout')}>
          Back to Workout
        </button>
      </div>

      <header className="hero">
        <div className="spark" aria-hidden="true" />
        <h1>Subscription</h1>
        <p className="lede">Un singur plan simplu: 40 RON/luna pentru acces complet la analizor.</p>
      </header>

      {error && <div className="error-banner">{error}</div>}

      {subscription ? (
        <section className="subscription-card">
          <SubscriptionManagement 
            subscription={subscription}
            onSubscriptionChange={loadSubscription}
          />
        </section>
      ) : (
        <section className="subscription-card">
          <h2>Activeaza abonamentul</h2>
          <p className="subscription-note">Deblochezi analiza completa pentru antrenamentele tale.</p>
          <PricingPlans 
            onSelectPlan={handleSelectPlan}
            isLoading={creatingSubscription}
          />
        </section>
      )}
    </div>
  )
}
