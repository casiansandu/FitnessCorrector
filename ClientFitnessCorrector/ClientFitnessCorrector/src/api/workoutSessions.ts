const API_BASE_URL = 'https://fitness-corrector-be.onrender.com/api'

export type AnalyzeSessionResponse = {
  message: string
  sessionId: string
  status: number
}

export type LandmarkPoint = {
  x: number
  y: number
  z: number
}

export type LandmarkFrame = LandmarkPoint[]

export type LineSegment = {
  start: number
  end: number
}

export type FrameAnalysis = {
  frame_index: number
  timestamp_ms: number
  bad_form: boolean | null
  failed_checks: string[]
  red_lines: LineSegment[]
  green_lines: LineSegment[]
}

export type WorkoutLandmarksResponse = {
  workout_session_id: string
  exercise_name: string
  fps: number
  landmarks: LandmarkFrame[]
  tracked_lines?: LineSegment[]
  frame_analysis?: FrameAnalysis[]
}

export type WorkoutSessionAdminDto = {
  id: string
  userId: string
  exerciseId: string
  videoFilePath: string
  status: number | string
  aiFeedback?: string | null
  outputPath?: string | null
  createdAt: string
}

export type TrialUsageResponse = {
  isSubscriber: boolean
  totalCount: number
  remainingCount: number
  limit: number
}

type AnalyzeParams = {
  exerciseId: string
  slug: string
  file: File
}

const handleResponse = async <T>(response: Response): Promise<T> => {
  if (!response.ok) {
    const errorText = await response.text()
    throw new Error(errorText || `Request failed with status ${response.status}`)
  }
  return response.json() as Promise<T>
}

export async function analyzeWorkoutSession({ exerciseId, slug, file }: AnalyzeParams): Promise<AnalyzeSessionResponse> {
  const formData = new FormData()
  formData.append('ExerciseId', exerciseId)
  formData.append('Slug', slug)
  formData.append('VideoFile', file)

  const response = await fetch(`${API_BASE_URL}/WorkoutSessions/analyze`, {
    method: 'POST',
    credentials: 'include',
    body: formData,
  })

  return handleResponse<AnalyzeSessionResponse>(response)
}

export async function fetchWorkoutLandmarks(sessionId: string): Promise<WorkoutLandmarksResponse> {
  const response = await fetch(`${API_BASE_URL}/WorkoutSessions/${sessionId}/landmarks`, {
    method: 'GET',
    credentials: 'include',
  })
  return handleResponse<WorkoutLandmarksResponse>(response)
}

export async function fetchAdminWorkoutSessions(take = 25): Promise<WorkoutSessionAdminDto[]> {
  const response = await fetch(`${API_BASE_URL}/WorkoutSessions/admin-sessions?take=${take}`, {
    method: 'GET',
    credentials: 'include',
  })
  return handleResponse<WorkoutSessionAdminDto[]>(response)
}

export async function fetchTrialUsage(): Promise<TrialUsageResponse> {
  const response = await fetch(`${API_BASE_URL}/WorkoutSessions/trial-usage`, {
    method: 'GET',
    credentials: 'include',
  })
  return handleResponse<TrialUsageResponse>(response)
}
