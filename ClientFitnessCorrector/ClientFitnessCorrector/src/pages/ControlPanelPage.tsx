import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { createExercise, fetchExercises, type CreateExercisePayload, type ExerciseOption } from '../api/exercises'
import { fetchAdminWorkoutSessions, type WorkoutSessionAdminDto } from '../api/workoutSessions'

export function ControlPanelPage() {
  const [exerciseList, setExerciseList] = useState<ExerciseOption[]>([])
  const [sessions, setSessions] = useState<WorkoutSessionAdminDto[]>([])
  const [statusMessage, setStatusMessage] = useState('Loading admin data...')
  const [formState, setFormState] = useState<CreateExercisePayload>({
    slug: '',
    name: '',
    description: '',
    muscleGroup: 'Legs',
  })
  const [isSubmitting, setIsSubmitting] = useState(false)

  const muscleGroups = useMemo(
    () => ['Chest', 'Back', 'Legs', 'Arms', 'Shoulders', 'Core', 'Cardio'],
    []
  )

  const exerciseLookup = useMemo(() => {
    return exerciseList.reduce<Record<string, ExerciseOption>>((acc, exercise) => {
      acc[exercise.id] = exercise
      return acc
    }, {})
  }, [exerciseList])

  useEffect(() => {
    const loadAdminData = async () => {
      try {
        const [exercises, sessionData] = await Promise.all([
          fetchExercises(),
          fetchAdminWorkoutSessions(25),
        ])
        setExerciseList(exercises)
        setSessions(sessionData)
        setStatusMessage('')
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Failed to load admin data.'
        setStatusMessage(message)
      }
    }

    void loadAdminData()
  }, [])

  const handleFormChange = (field: keyof CreateExercisePayload, value: string) => {
    setFormState((prev) => ({ ...prev, [field]: value }))
  }

  const handleCreateExercise = async () => {
    if (!formState.slug || !formState.name || !formState.description) {
      setStatusMessage('Fill out slug, name, and description before saving.')
      return
    }

    setIsSubmitting(true)
    try {
      await createExercise(formState)
      const refreshed = await fetchExercises()
      setExerciseList(refreshed)
      setFormState((prev) => ({ ...prev, slug: '', name: '', description: '' }))
      setStatusMessage('Exercise created.')
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Failed to create exercise.'
      setStatusMessage(message)
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="page-shell">
      <header className="hero">
        <div className="spark" aria-hidden="true" />
        <h1>Control Panel</h1>
        <p className="lede">Manage exercises and keep an eye on recent session activity.</p>
      </header>

      {statusMessage ? <p className="status-text">{statusMessage}</p> : null}

      <section className="control-panel-grid">
        <div className="control-panel-card">
          <h2>Exercise management</h2>
          <p>Create new exercises to unlock additional analysis modes.</p>

          <div className="admin-form">
            <label>
              Slug
              <input
                value={formState.slug}
                onChange={(event) => handleFormChange('slug', event.target.value)}
                placeholder="bench-press"
              />
            </label>
            <label>
              Name
              <input
                value={formState.name}
                onChange={(event) => handleFormChange('name', event.target.value)}
                placeholder="Bench Press"
              />
            </label>
            <label>
              Description
              <textarea
                value={formState.description}
                onChange={(event) => handleFormChange('description', event.target.value)}
                placeholder="Barbell bench press"
              />
            </label>
            <label>
              Muscle group
              <select
                value={formState.muscleGroup}
                onChange={(event) => handleFormChange('muscleGroup', event.target.value)}
              >
                {muscleGroups.map((group) => (
                  <option key={group} value={group}>
                    {group}
                  </option>
                ))}
              </select>
            </label>
          </div>

          <button type="button" onClick={handleCreateExercise} disabled={isSubmitting}>
            {isSubmitting ? 'Saving...' : 'Create exercise'}
          </button>

          <div className="admin-list">
            {exerciseList.map((exercise) => (
              <div key={exercise.id} className="admin-list__row">
                <div>
                  <strong>{exercise.name}</strong>
                  <span>{exercise.slug}</span>
                </div>
                <span>{exercise.muscleGroup}</span>
              </div>
            ))}
          </div>
        </div>

        <div className="control-panel-card">
          <h2>Recent sessions</h2>
          <p>Latest workout analyses across all users.</p>
          <div className="admin-list">
            {sessions.map((session) => {
              const exercise = exerciseLookup[session.exerciseId]
              return (
              <div key={session.id} className="admin-list__row">
                <div>
                  <strong>{exercise?.name ?? session.exerciseId}</strong>
                  <span>{new Date(session.createdAt).toLocaleString()}</span>
                </div>
                <span>{String(session.status)}</span>
              </div>
              )
            })}
          </div>
        </div>
      </section>

      <Link className="control-panel-back" to="/workout">
        Back to Workout
      </Link>
    </div>
  )
}
