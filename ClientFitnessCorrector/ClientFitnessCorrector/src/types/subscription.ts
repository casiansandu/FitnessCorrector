export const PlanType = {
  Basic: 'Basic',
  Premium: 'Premium',
  Pro: 'Pro'
} as const

export type PlanType = (typeof PlanType)[keyof typeof PlanType]

export const SubscriptionStatus = {
  Active: 'Active',
  Canceled: 'Canceled',
  PastDue: 'PastDue',
  Incomplete: 'Incomplete',
  Trialing: 'Trialing'
} as const

export type SubscriptionStatus = (typeof SubscriptionStatus)[keyof typeof SubscriptionStatus]

export type Subscription = {
  id: string
  userId: string
  stripeSubscriptionId: string
  planType: PlanType
  status: SubscriptionStatus
  currentPeriodStart: Date
  currentPeriodEnd: Date
  cancelAtPeriodEnd: boolean
  createdAt: Date
  updatedAt?: Date
  checkoutUrl?: string
}

export type PlanPricing = {
  planType: PlanType
  priceInCents: number
  description: string
  features: string[]
}

export type CreateSubscriptionPayload = {
  planType: PlanType
}

export type CreateSubscriptionCheckoutResponse = {
  checkoutUrl: string
}

export type ChangePlanPayload = {
  stripeSubscriptionId: string
  newPlanType: PlanType
}

export type CancelSubscriptionPayload = {
  stripeSubscriptionId: string
}
