const API_BASE_URL = 'https://fitness-corrector-be.onrender.com/api'

export type ProgressPoint = {
  date: string
  averageDepth: number
  averageTempoSeconds: number
  averageSymmetry: number
  repCount: number
}

export type ProgressHighlight = {
  message: string
  metric: string
  delta: number
}

const handleResponse = async <T>(response: Response): Promise<T> => {
  if (!response.ok) {
    const errorText = await response.text().catch(() => '')
    throw new Error(errorText || `Request failed with status ${response.status}`)
  }
  return response.json() as Promise<T>
}

export async function fetchProgress(exerciseId: string, rangeDays = 30): Promise<ProgressPoint[]> {
  const response = await fetch(`${API_BASE_URL}/WorkoutSessions/progress?exerciseId=${exerciseId}&rangeDays=${rangeDays}`, {
    method: 'GET',
    credentials: 'include',
  })

  return handleResponse<ProgressPoint[]>(response)
}

export async function fetchHighlights(exerciseId: string): Promise<ProgressHighlight> {
  const response = await fetch(`${API_BASE_URL}/WorkoutSessions/highlights?exerciseId=${exerciseId}`, {
    method: 'GET',
    credentials: 'include',
  })

  return handleResponse<ProgressHighlight>(response)
}
