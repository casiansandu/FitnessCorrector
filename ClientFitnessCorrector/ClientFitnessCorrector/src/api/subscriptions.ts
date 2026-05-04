import {
  type CreateSubscriptionPayload,
  type CreateSubscriptionCheckoutResponse,
  type ChangePlanPayload,
  type CancelSubscriptionPayload,
  type Subscription,
  type PlanPricing
} from '../types/subscription'

const API_BASE_URL = 'http://localhost:5168/api'

const extractApiErrorMessage = (errorPayload: unknown, fallback: string): string => {
  if (errorPayload && typeof errorPayload === 'object') {
    const payload = errorPayload as { message?: unknown; errors?: unknown }

    if (typeof payload.message === 'string' && payload.message.trim().length > 0) {
      return payload.message
    }

    if (payload.errors && typeof payload.errors === 'object') {
      const firstError = Object.values(payload.errors as Record<string, unknown>)[0]
      if (Array.isArray(firstError) && firstError.length > 0 && typeof firstError[0] === 'string') {
        return firstError[0]
      }
    }
  }

  return fallback
}

export const subscriptionApi = {
  async getPricingPlans(): Promise<PlanPricing[]> {
    const response = await fetch(`${API_BASE_URL}/subscriptions/pricing-plans`)
    if (!response.ok) {
      throw new Error('Failed to fetch pricing plans')
    }
    return response.json()
  },

  async createSubscription(payload: CreateSubscriptionPayload): Promise<CreateSubscriptionCheckoutResponse> {
    const response = await fetch(`${API_BASE_URL}/subscriptions/create`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      credentials: 'include',
      body: JSON.stringify(payload)
    })

    if (!response.ok) {
      const error = await response.json()
      throw new Error(extractApiErrorMessage(error, 'Failed to create subscription'))
    }

    return response.json()
  },

  async getMySubscription(): Promise<Subscription | null> {
    const response = await fetch(`${API_BASE_URL}/subscriptions/my-subscription`, {
      credentials: 'include'
    })

    if (response.status === 404) {
      return null
    }

    if (!response.ok) {
      throw new Error('Failed to fetch subscription')
    }

    return response.json()
  },

  async changePlan(payload: ChangePlanPayload): Promise<{ success: boolean }> {
    const response = await fetch(`${API_BASE_URL}/subscriptions/change-plan`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      credentials: 'include',
      body: JSON.stringify(payload)
    })

    if (!response.ok) {
      const error = await response.json()
      throw new Error(extractApiErrorMessage(error, 'Failed to change plan'))
    }

    return response.json()
  },

  async cancelSubscription(payload: CancelSubscriptionPayload): Promise<{ success: boolean }> {
    const query = payload.stripeSubscriptionId
      ? `?stripeSubscriptionId=${encodeURIComponent(payload.stripeSubscriptionId)}`
      : ''

    const response = await fetch(`${API_BASE_URL}/subscriptions/cancel${query}`, {
      method: 'DELETE',
      credentials: 'include'
    })

    if (!response.ok) {
      const error = await response.json()
      throw new Error(extractApiErrorMessage(error, 'Failed to cancel subscription'))
    }

    return response.json()
  }
}
