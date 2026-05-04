import { useState } from 'react'
import type { Subscription } from '../types/subscription'
import { subscriptionApi } from '../api/subscriptions'

interface SubscriptionManagementProps {
  subscription: Subscription | null
  onSubscriptionChange: () => void
}

export function SubscriptionManagement({ subscription, onSubscriptionChange }: Readonly<SubscriptionManagementProps>) {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleCancelSubscription = async () => {
    if (!subscription) return

    const confirmed = globalThis.confirm(
      'Are you sure you want to cancel your subscription? You will lose access at the end of this billing period.'
    )

    if (!confirmed) return

    try {
      setLoading(true)
      setError(null)
      await subscriptionApi.cancelSubscription({
        stripeSubscriptionId: subscription.stripeSubscriptionId
      })
      onSubscriptionChange()
      alert('Subscription canceled successfully. You will lose access at the end of your billing period.')
    } catch (err) {
      setError((err as Error).message || 'Failed to cancel subscription')
    } finally {
      setLoading(false)
    }
  }

  if (!subscription) {
    return null
  }

  const endDate = new Date(subscription.currentPeriodEnd)
  const now = new Date()
  const daysLeft = Math.ceil((endDate.getTime() - now.getTime()) / (1000 * 60 * 60 * 24))

  return (
    <div className="subscription-management">
      <div className="subscription-info">
        <h3>Your Current Plan</h3>
        
        <div className="plan-details">
          <p><strong>Plan:</strong> {subscription.planType}</p>
          <p><strong>Status:</strong> {subscription.status}</p>
          <p><strong>Billing Period:</strong> {new Date(subscription.currentPeriodStart).toLocaleDateString()} - {endDate.toLocaleDateString()}</p>
          <p><strong>Days Left:</strong> {daysLeft} days</p>
        </div>

        {subscription.cancelAtPeriodEnd && (
          <div className="cancellation-notice">
            ⚠️ Your subscription is scheduled to cancel at the end of the billing period.
          </div>
        )}

        {error && <div className="error-message">{error}</div>}

        {!subscription.cancelAtPeriodEnd && (
          <button
            onClick={handleCancelSubscription}
            disabled={loading}
            className="cancel-button"
          >
            {loading ? 'Canceling...' : 'Cancel Subscription'}
          </button>
        )}
      </div>
    </div>
  )
}
