import { useState } from 'react'
import { PlanType } from '../types/subscription'
import { subscriptionApi } from '../api/subscriptions'

interface UpgradePlanProps {
  currentPlanType: PlanType
  onUpgradeSuccess: () => void
}

export function UpgradePlan({ currentPlanType, onUpgradeSuccess }: UpgradePlanProps) {
  const [selectedPlan, setSelectedPlan] = useState<PlanType | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const planHierarchy = [PlanType.Basic, PlanType.Premium, PlanType.Pro]
  const currentPlanIndex = planHierarchy.indexOf(currentPlanType)

  const handleChangePlan = async () => {
    if (!selectedPlan) return

    try {
      setLoading(true)
      setError(null)
      
      // This assumes we have stripeSubscriptionId - in real app you'd get it from subscription object
      await subscriptionApi.changePlan({
        stripeSubscriptionId: '', // You need to pass the actual ID from parent
        newPlanType: selectedPlan
      })

      onUpgradeSuccess()
      alert('Plan changed successfully!')
    } catch (err) {
      setError((err as Error).message || 'Failed to change plan')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="upgrade-plan">
      <h4>Change Your Plan</h4>
      
      {error && <div className="error-message">{error}</div>}

      <div className="available-plans">
        {planHierarchy.map((plan, idx) => (
          <div
            key={plan}
            className={`plan-option ${selectedPlan === plan ? 'selected' : ''} ${idx <= currentPlanIndex ? 'disabled' : ''}`}
          >
            <input
              type="radio"
              name="new-plan"
              value={plan}
              checked={selectedPlan === plan}
              onChange={() => setSelectedPlan(plan)}
              disabled={idx <= currentPlanIndex}
            />
            <label>{plan}</label>
          </div>
        ))}
      </div>

      { selectedPlan && (
        <button
          onClick={handleChangePlan}
          disabled={loading || !selectedPlan}
          className="change-plan-button"
        >
          {loading ? 'Processing...' : 'Change Plan'}
        </button>
      )}
    </div>
  )
}
