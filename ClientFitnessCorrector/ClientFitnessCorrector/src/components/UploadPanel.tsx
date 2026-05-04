import type { ChangeEvent } from 'react'

type UploadPanelProps = {
  fileName: string
  statusMessage: string
  isSending: boolean
  hasVideo: boolean
  canSend: boolean
  hasOverlay: boolean
  exerciseOptions: Array<{ value: string; label: string }>
  selectedExercise: string
  onFileChange: (event: ChangeEvent<HTMLInputElement>) => void
  onExerciseChange: (value: string) => void
  onSend: () => void
  onClearOverlay: () => void
}

export function UploadPanel({
  fileName,
  statusMessage,
  isSending,
  hasVideo,
  canSend,
  hasOverlay,
  exerciseOptions,
  selectedExercise,
  onFileChange,
  onExerciseChange,
  onSend,
  onClearOverlay,
}: UploadPanelProps) {
  const handleSelectChange = (event: ChangeEvent<HTMLSelectElement>) => {
    onExerciseChange(event.target.value)
  }

  return (
    <div className="upload-card">
      <label className="upload-field">
        <input type="file" accept="video/*" onChange={onFileChange} />
        <span>{fileName || 'Drop or browse a video file'}</span>
      </label>

      <div className="field-group">
        <label className="field-label" htmlFor="exercise-select">
          Exercise focus
        </label>
        <div className="select-shell">
          <select id="exercise-select" value={selectedExercise} onChange={handleSelectChange}>
            {exerciseOptions.map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
        </div>
        <p className="field-hint">We will tune pose scoring for the movement you choose.</p>
      </div>

      <div className="actions">
        <button onClick={onSend} disabled={isSending || !hasVideo || !canSend}>
          {isSending ? 'Sending...' : 'Send to server'}
        </button>
        <button onClick={onClearOverlay} disabled={!hasOverlay} className="ghost">
          Clear overlay
        </button>
      </div>

      <p className="status-text">{statusMessage}</p>
      {/* {sessionId && <p className="session-id">Session ID: {sessionId}</p>} */}
    </div>
  )
}
