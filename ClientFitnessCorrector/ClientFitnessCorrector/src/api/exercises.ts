const API_BASE_URL = 'http://localhost:5168/api'

export type ExerciseOption = {
  id: string
  name: string
  slug: string
  muscleGroup: string
}

export type CreateExercisePayload = {
  slug: string
  name: string
  description: string
  muscleGroup: string
}

export async function fetchExercises(): Promise<ExerciseOption[]> {
  const response = await fetch(`${API_BASE_URL}/Exercises`, {
    method: 'GET',
    credentials: 'include',
  })

  if (!response.ok) {
    const errorText = await response.text().catch(() => '')
    throw new Error(errorText || 'Failed to load exercises.')
  }

  return response.json() as Promise<ExerciseOption[]>
}

export async function createExercise(payload: CreateExercisePayload): Promise<string> {
  const response = await fetch(`${API_BASE_URL}/Exercises`, {
    method: 'POST',
    credentials: 'include',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      slug: payload.slug,
      name: payload.name,
      description: payload.description,
      muscleGroup: payload.muscleGroup,
    }),
  })

  if (!response.ok) {
    const errorText = await response.text().catch(() => '')
    throw new Error(errorText || 'Failed to create exercise.')
  }

  const data = (await response.json().catch(() => ({}))) as { id?: string }
  return data.id ?? ''
}
