const API_BASE_URL = 'http://localhost:5168/api'

type ValidationIssue = {
  property: string
  message: string
}

type ValidationErrorResponse = {
  errors?: ValidationIssue[]
}

type ConflictErrorResponse = {
  message?: string
}

type UnauthorizedErrorResponse = {
  message?: string
}

export type RegisterUserPayload = {
  email: string
  passwordHash: string
  firstName: string
  lastName: string
}

export type RegisterUserResponse = {
  userId: string
  email: string
  firstName: string
  lastName: string
  token: string
}

export type LoginUserPayload = {
  email: string
  passwordHash: string
}

export type LoginUserResponse = {
  userId: string
  email: string
  firstName?: string
  lastName?: string
  token?: string
  role?: string
}

type LogoutResponse = {
  message?: string
}

export async function registerUser(payload: RegisterUserPayload): Promise<RegisterUserResponse> {
  const response = await fetch(`${API_BASE_URL}/Auth/register`, {
    method: 'POST',
    credentials: 'include',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      Email: payload.email,
      PasswordHash: payload.passwordHash,
      FirstName: payload.firstName,
      LastName: payload.lastName,
    }),
  })

  if (response.ok) {
    return response.json() as Promise<RegisterUserResponse>
  }

  if (response.status === 400) {
    const data = (await response.json().catch(() => null)) as ValidationErrorResponse | null
    const messages = data?.errors?.map((error) => error.message).filter(Boolean)
    throw new Error(messages?.length ? messages.join(' ') : 'Invalid registration details.')
  }

  if (response.status === 409) {
    const data = (await response.json().catch(() => null)) as ConflictErrorResponse | null
    throw new Error(data?.message || 'User with this email already exists.')
  }

  const fallbackText = await response.text().catch(() => '')
  throw new Error(fallbackText || 'Registration failed. Try again.')
}

export async function loginUser(payload: LoginUserPayload): Promise<LoginUserResponse> {
  const response = await fetch(`${API_BASE_URL}/Auth/login`, {
    method: 'POST',
    credentials: 'include',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      email: payload.email,
      passwordHash: payload.passwordHash,
    }),
  })

  if (response.ok) {
    const data = (await response.json()) as Record<string, unknown>

    return {
      userId: String(data.userId ?? data.id ?? ''),
      email: String(data.email ?? ''),
      firstName: typeof data.firstName === 'string' ? data.firstName : undefined,
      lastName: typeof data.lastName === 'string' ? data.lastName : undefined,
      token: typeof data.token === 'string' ? data.token : undefined,
      role: typeof data.role === 'string' ? data.role : typeof data.Role === 'string' ? data.Role : undefined,
    }
  }

  if (response.status === 400) {
    const data = (await response.json().catch(() => null)) as ValidationErrorResponse | null
    const messages = data?.errors?.map((error) => error.message).filter(Boolean)
    throw new Error(messages?.length ? messages.join(' ') : 'Invalid login details.')
  }

  if (response.status === 401) {
    const data = (await response.json().catch(() => null)) as UnauthorizedErrorResponse | null
    throw new Error(data?.message || 'Invalid credentials.')
  }

  const fallbackText = await response.text().catch(() => '')
  throw new Error(fallbackText || 'Login failed. Try again.')
}

export async function logoutUser(): Promise<LogoutResponse> {
  const response = await fetch(`${API_BASE_URL}/Auth/logout`, {
    method: 'POST',
    credentials: 'include',
  })

  if (response.ok) {
    return (await response.json().catch(() => ({}))) as LogoutResponse
  }

  const fallbackText = await response.text().catch(() => '')
  throw new Error(fallbackText || 'Logout failed. Try again.')
}