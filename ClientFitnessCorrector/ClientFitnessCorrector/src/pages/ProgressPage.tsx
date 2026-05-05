import { useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { fetchExercises, type ExerciseOption } from '../api/exercises'
import { fetchHighlights, fetchProgress, type ProgressPoint } from '../api/progress'
import { logoutUser } from '../api/auth'
import { ProgressChart } from '../components/ProgressChart'

const USER_ID_STORAGE_KEY = 'fitnessCorrectorUserId'
const IS_ADMIN_STORAGE_KEY = 'fitnessCorrectorIsAdmin'

const RANGE_OPTIONS = [
  { label: '7 days', value: 7 },
  { label: '30 days', value: 30 },
  { label: '90 days', value: 90 },
]

const METRIC_OPTIONS = [
  {
    label: 'Depth',
    value: 'averageDepth',
    unit: 'ratio',
    axisLabel: 'Depth (0-1)',
    description: 'Depth ratio (0-1). Higher means deeper range of motion.',
  },
  {
    label: 'Tempo',
    value: 'averageTempoSeconds',
    unit: 'seconds',
    axisLabel: 'Tempo (sec)',
    description: 'Rep duration in seconds. Lower means faster reps.',
  },
  {
    label: 'Symmetry',
    value: 'averageSymmetry',
    unit: 'ratio',
    axisLabel: 'Symmetry (0-1)',
    description: 'Left/right balance (0-1). Higher means more even.',
  },
] as const

type MetricKey = (typeof METRIC_OPTIONS)[number]['value']

export function ProgressPage() {
  const navigate = useNavigate()
  const [isAdmin, setIsAdmin] = useState(localStorage.getItem(IS_ADMIN_STORAGE_KEY) === 'true')
  const [exercises, setExercises] = useState<ExerciseOption[]>([])
  const [selectedExerciseId, setSelectedExerciseId] = useState<string>('')
  const [rangeDays, setRangeDays] = useState(30)
  const [metricKey, setMetricKey] = useState<MetricKey>('averageDepth')
  const [progress, setProgress] = useState<ProgressPoint[]>([])
  const [highlight, setHighlight] = useState<string>('Complete two sessions to unlock highlights.')
  const [statusMessage, setStatusMessage] = useState('Loading progress...')

  useEffect(() => {
    const loadExercises = async () => {
      try {
        const data = await fetchExercises()
        setExercises(data)
        if (data.length > 0) {
          setSelectedExerciseId(data[0].id)
        }
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Failed to load exercises.'
        setStatusMessage(message)
      }
    }

    void loadExercises()
  }, [])

  useEffect(() => {
    const loadProgress = async () => {
      if (!selectedExerciseId) {
        return
      }

      setStatusMessage('Loading progress...')
      try {
        const data = await fetchProgress(selectedExerciseId, rangeDays)
        setProgress(data)
        const highlightData = await fetchHighlights(selectedExerciseId)
        setHighlight(highlightData.message)
        setStatusMessage('')
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Failed to load progress.'
        setStatusMessage(message)
      }
    }

    void loadProgress()
  }, [selectedExerciseId, rangeDays])

  const chartPoints = useMemo(() => {
    return progress.map((point) => ({
      date: new Date(point.date).toLocaleDateString(),
      value: point[metricKey] ?? 0,
    }))
  }, [progress, metricKey])

  const selectedMetric = METRIC_OPTIONS.find((metric) => metric.value === metricKey)

  const handleLogout = async () => {
    try {
      await logoutUser()
    } catch {
      // Keep local cleanup and redirect even if backend logout fails.
    }

    localStorage.removeItem(USER_ID_STORAGE_KEY)
    localStorage.removeItem(IS_ADMIN_STORAGE_KEY)
    setIsAdmin(false)
    navigate('/login', { replace: true })
  }

  return (
    <div className="page-shell">
      <div className="top-actions">
        <button type="button" className="subscription-chip" onClick={() => navigate('/subscriptions')}>
          Subscription
        </button>
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
        <p className="eyebrow">Progress</p>
        <h1>Trends that actually matter</h1>
        <p className="lede">Track movement depth, tempo, and symmetry across your recent sessions.</p>
      </header>

      <section className="progress-shell">
        <div className="progress-card">
          <div className="progress-controls">
            <label>
              Exercise
              <select value={selectedExerciseId} onChange={(event) => setSelectedExerciseId(event.target.value)}>
                {exercises.map((exercise) => (
                  <option key={exercise.id} value={exercise.id}>
                    {exercise.name}
                  </option>
                ))}
              </select>
            </label>
            <label>
              Metric
              <select value={metricKey} onChange={(event) => setMetricKey(event.target.value as MetricKey)}>
                {METRIC_OPTIONS.map((metric) => (
                  <option key={metric.value} value={metric.value}>
                    {metric.label}
                  </option>
                ))}
              </select>
            </label>
            <label>
              Range
              <select value={rangeDays} onChange={(event) => setRangeDays(Number(event.target.value))}>
                {RANGE_OPTIONS.map((range) => (
                  <option key={range.value} value={range.value}>
                    {range.label}
                  </option>
                ))}
              </select>
            </label>
          </div>

          {statusMessage ? <p className="status-text">{statusMessage}</p> : null}

          <ProgressChart
            title={selectedMetric?.label ?? 'Metric'}
            unit={selectedMetric?.unit}
            yLabel={selectedMetric?.axisLabel ?? ''}
            xLabel="Session date"
            points={chartPoints}
          />
          {selectedMetric?.description ? (
            <p className="progress-chart__note">{selectedMetric.description}</p>
          ) : null}
        </div>

        <div className="progress-highlight">
          <h3>Latest improvement</h3>
          <p>{highlight}</p>
          <button type="button" className="ghost" onClick={() => navigate('/workout')}>
            Analyze another workout
          </button>
        </div>
      </section>
    </div>
  )
}
