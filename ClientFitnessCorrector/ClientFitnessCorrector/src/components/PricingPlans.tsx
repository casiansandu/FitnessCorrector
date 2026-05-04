import { useEffect, useState } from 'react'
import { type PlanPricing, PlanType } from '../types/subscription'
import { subscriptionApi } from '../api/subscriptions'

interface PricingPlansProps {
  onSelectPlan: (planType: PlanType) => void
  currentPlan?: PlanType
  isLoading?: boolean
}

export function PricingPlans({ onSelectPlan, currentPlan, isLoading = false }: PricingPlansProps) {
  const [plans, setPlans] = useState<PlanPricing[]>([])
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    const loadPlans = async () => {
      try {
        setLoading(true)
        const data = await subscriptionApi.getPricingPlans()
        setPlans(data)
      } catch (error) {
        console.error('Failed to load pricing plans:', error)
      } finally {
        setLoading(false)
      }
    }

    loadPlans()
  }, [])

  if (loading) {
    return <div className="pricing-loading">Loading plans...</div>
  }

  const plan = plans[0]

  if (!plan) {
    return <div className="pricing-loading">No subscription plan available right now.</div>
  }

  return (
    <div className="pricing-plans">
      <div
        className={`pricing-card ${currentPlan === plan.planType ? 'active' : ''}`}
      >
        <h3>Fitness Corrector Monthly</h3>
        <div className="price">
          {(plan.priceInCents / 100).toFixed(0)} RON/luna
        </div>
        <p className="description">{plan.description}</p>

        <ul className="features">
          {plan.features.map((feature, idx) => (
            <li key={idx}>✓ {feature}</li>
          ))}
        </ul>

        <button
          onClick={() => onSelectPlan(plan.planType as PlanType)}
          disabled={isLoading || currentPlan === plan.planType}
          className="plan-button"
        >
          {currentPlan === plan.planType ? 'Abonament activ' : 'Activeaza abonamentul'}
        </button>
      </div>
    </div>
  )
}
