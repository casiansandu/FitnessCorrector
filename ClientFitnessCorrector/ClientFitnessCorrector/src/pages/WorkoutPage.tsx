import { useEffect, useState } from 'react'
import type { ChangeEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { UploadPanel } from '../components/UploadPanel'
import { VideoStage } from '../components/VideoStage'
import type { PoseStroke } from '../types/pose'
import { analyzeWorkoutSession, fetchWorkoutLandmarks, fetchTrialUsage, type TrialUsageResponse } from '../api/workoutSessions'
import type { FrameAnalysis, LandmarkFrame, LineSegment } from '../api/workoutSessions'
import { fetchExercises, type ExerciseOption } from '../api/exercises'
import { logoutUser } from '../api/auth'
import { subscriptionApi } from '../api/subscriptions'
import { SubscriptionStatus, type Subscription } from '../types/subscription'

const USER_ID_STORAGE_KEY = 'fitnessCorrectorUserId'
const IS_ADMIN_STORAGE_KEY = 'fitnessCorrectorIsAdmin'

type Exercise = {
  id: string
  value: string
  label: string
}

const FALLBACK_EXERCISES: Exercise[] = [
  { id: 'squat', value: 'squat', label: 'Back Squat' },
  { id: 'deadlift', value: 'deadlift', label: 'Deadlift' },
  { id: 'bench-press', value: 'bench-press', label: 'Bench Press' },
]

export function WorkoutPage() {
  const navigate = useNavigate()
  const [isAdmin, setIsAdmin] = useState(localStorage.getItem(IS_ADMIN_STORAGE_KEY) === 'true')
  const [videoUrl, setVideoUrl] = useState<string | null>(null)
  const [videoFile, setVideoFile] = useState<File | null>(null)
  const [fileName, setFileName] = useState('')
  const [statusMessage, setStatusMessage] = useState('Upload a movement clip to get started.')
  const [overlay, setOverlay] = useState<PoseStroke[]>([])
  const [landmarkFrames, setLandmarkFrames] = useState<LandmarkFrame[]>([])
  const [landmarkFps, setLandmarkFps] = useState<number | null>(null)
  const [trackedLines, setTrackedLines] = useState<LineSegment[]>([])
  const [frameAnalysis, setFrameAnalysis] = useState<FrameAnalysis[]>([])
  const [isSending, setIsSending] = useState(false)
  const [exerciseOptions, setExerciseOptions] = useState<Exercise[]>(FALLBACK_EXERCISES)
  const [selectedExercise, setSelectedExercise] = useState(FALLBACK_EXERCISES[0])
  const [subscription, setSubscription] = useState<Subscription | null>(null)
  const [trialUsage, setTrialUsage] = useState<TrialUsageResponse | null>(null)

  const hasActiveSubscription = subscription?.status === SubscriptionStatus.Active || subscription?.status === SubscriptionStatus.Trialing
  const hasTrialAccess = subscription == null
  const canAnalyze = hasActiveSubscription || hasTrialAccess

  useEffect(() => {
    return () => {
      if (videoUrl) {
        URL.revokeObjectURL(videoUrl)
      }
    }
  }, [videoUrl])

  useEffect(() => {
    const loadSubscription = async () => {
      try {
        const data = await subscriptionApi.getMySubscription()
        setSubscription(data)
      } catch {
        setSubscription(null)
      }
    }

    void loadSubscription()
  }, [])

  useEffect(() => {
    const loadTrialUsage = async () => {
      try {
        const data = await fetchTrialUsage()
        setTrialUsage(data)
      } catch {
        setTrialUsage(null)
      }
    }

    void loadTrialUsage()
  }, [subscription])

  useEffect(() => {
    const loadExercises = async () => {
      try {
        const data = await fetchExercises()
        if (data.length === 0) {
          return
        }

        const mapped: Exercise[] = data.map((exercise: ExerciseOption) => ({
          id: exercise.id,
          value: exercise.slug,
          label: exercise.name,
        }))

        setExerciseOptions(mapped)
        setSelectedExercise(mapped[0])
      } catch {
        setExerciseOptions(FALLBACK_EXERCISES)
        setSelectedExercise(FALLBACK_EXERCISES[0])
      }
    }

    void loadExercises()
  }, [])

  const handleFileChange = (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    if (!file) {
      return
    }

    const nextUrl = URL.createObjectURL(file)
    setVideoUrl((prev) => {
      if (prev) {
        URL.revokeObjectURL(prev)
      }
      return nextUrl
    })
    setVideoFile(file)
    setFileName(file.name)
    setOverlay([])
    setLandmarkFrames([])
    setLandmarkFps(null)
    setTrackedLines([])
    setFrameAnalysis([])
    const exerciseLabel = exerciseOptions.find((option) => option.value === selectedExercise.value)?.label ?? 'exercise'
    setStatusMessage(`Ready to send this clip for a ${exerciseLabel.toLowerCase()} analysis.`)
  }

  const handleSendToServer = async () => {
    if (!videoFile) {
      setStatusMessage('Please upload a video before sending.')
      return
    }

    if (!canAnalyze) {
      setStatusMessage('You need an active subscription before sending videos for analysis.')
      navigate('/subscriptions')
      return
    }

    setIsSending(true)
    const exerciseLabel = exerciseOptions.find((option) => option.value === selectedExercise.value)?.label ?? 'training'
    setStatusMessage(`Sending your ${exerciseLabel.toLowerCase()} clip to the pose model...`)

    try {
      const workoutSession = await analyzeWorkoutSession({
        exerciseId: selectedExercise.id,
        slug: selectedExercise.value,
        file: videoFile,
      })

      const nextSessionId = workoutSession.sessionId

      console.log('Workout session response:', workoutSession)

      const landmarksResponse = await fetchWorkoutLandmarks(nextSessionId)
      setLandmarkFrames(landmarksResponse.landmarks)
      setLandmarkFps(typeof landmarksResponse.fps === 'number' ? landmarksResponse.fps : null)
      setTrackedLines(landmarksResponse.tracked_lines ?? [])
      setFrameAnalysis(landmarksResponse.frame_analysis ?? [])
      setOverlay([])
      setStatusMessage(`${exerciseLabel} pose overlay received. Ready for review.`)

      try {
        const data = await fetchTrialUsage()
        setTrialUsage(data)
      } catch {
        setTrialUsage(null)
      }

      console.log('Landmarks response:', landmarksResponse.landmarks.length * landmarksResponse.landmarks[0].length, 'points received')
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Request failed. Try again.'
      if (message.includes('401')) {
        setStatusMessage('Your session has expired. Please sign in again.')
        navigate('/login', { replace: true })
        return
      }

      if (message.includes('403')) {
        setStatusMessage(message)
        navigate('/subscriptions')
        return
      }

      setStatusMessage(message)
    } finally {
      setIsSending(false)
    }
  }

  const handleClearOverlay = () => {
    setOverlay([])
    setLandmarkFrames([])
    setLandmarkFps(null)
    setTrackedLines([])
    setFrameAnalysis([])
    setStatusMessage('Overlay cleared. Ready for the next analysis.')
  }

  const handleExerciseChange = (value: string) => {
    setSelectedExercise(exerciseOptions.find((option) => option.value === value) || FALLBACK_EXERCISES[0])
    if (videoUrl) {
      const exerciseLabel = exerciseOptions.find((option) => option.value === value)?.label ?? 'exercise'
      setStatusMessage(`Ready to send this clip for a ${exerciseLabel.toLowerCase()} analysis.`)
    }
  }

  const handleLogout = async () => {
    try {
      await logoutUser()
    } catch {
      // Keep local cleanup and redirect even if backend logout fails.
    }

    localStorage.removeItem(USER_ID_STORAGE_KEY)
    localStorage.removeItem(IS_ADMIN_STORAGE_KEY)
    setIsAdmin(false)

    if (videoUrl) {
      URL.revokeObjectURL(videoUrl)
    }

    setVideoUrl(null)
    setVideoFile(null)
    setFileName('')
    setOverlay([])
    setLandmarkFrames([])
    setLandmarkFps(null)
    setTrackedLines([])
    setFrameAnalysis([])
    setIsSending(false)
    setStatusMessage('You have been logged out.')

    navigate('/login', { replace: true })
  }

  return (
    <div className="page-shell">
      <div className="top-actions">
        <button type="button" className="subscription-chip" onClick={() => navigate('/subscriptions')}>
          Subscription
        </button>
        <button type="button" className="progress-chip" onClick={() => navigate('/progress')}>
          Progress
        </button>
        {trialUsage && !trialUsage.isSubscriber ? (
          <div className="trial-chip">
            Trial {trialUsage.remainingCount}/{trialUsage.limit}
          </div>
        ) : null}
        {isAdmin ? (
          <button type="button" className="admin-chip" onClick={() => navigate('/control-panel')}>
            Control Panel
          </button>
        ) : null}
        <button type="button" className="logout-chip" onClick={handleLogout}>
          Logout
        </button>
      </div>
      <header className="hero">
        <div className="spark" aria-hidden="true" />
        <h1>Fitness Corrector</h1>
        <p className="lede">
          Upload a training clip, send it to your pose service, and preview the feedback.
        </p>
      </header>

      <section className="workspace">
        <div className="workspace-left">
          <UploadPanel
            fileName={fileName}
            statusMessage={statusMessage}
            isSending={isSending}
            hasVideo={Boolean(videoUrl)}
            canSend={canAnalyze}
            hasOverlay={overlay.length > 0 || landmarkFrames.length > 0}
            exerciseOptions={exerciseOptions}
            selectedExercise={selectedExercise.value}
            onFileChange={handleFileChange}
            onExerciseChange={handleExerciseChange}
            onSend={handleSendToServer}
            onClearOverlay={handleClearOverlay}
          />

          {trialUsage && !trialUsage.isSubscriber ? (
            <div className="trial-card">
              <h3>Free trial</h3>
              <p>
                Trial remaining: {trialUsage.remainingCount} of {trialUsage.limit} analyses.
              </p>
            </div>
          ) : null}
        </div>

        <div className="viewer-card">
          <VideoStage
            videoUrl={videoUrl}
            strokes={overlay}
            landmarkFrames={landmarkFrames}
            landmarkFps={landmarkFps}
            trackedLines={trackedLines}
            frameAnalysis={frameAnalysis}
          />
        </div>
      </section>
    </div>
  )
}
