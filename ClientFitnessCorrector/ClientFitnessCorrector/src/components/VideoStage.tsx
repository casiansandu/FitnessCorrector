import { useCallback, useEffect, useRef, useState } from 'react'
import type { FrameAnalysis, LandmarkFrame, LineSegment } from '../api/workoutSessions'
import type { PoseStroke } from '../types/pose'

type VideoStageProps = {
  videoUrl: string | null
  strokes: PoseStroke[]
  landmarkFrames?: LandmarkFrame[]
  landmarkFps?: number | null
  trackedLines?: LineSegment[]
  frameAnalysis?: FrameAnalysis[]
}

const DRAW_STYLE = {
  strokeStyle: '#30ffd8',
  lineWidth: 4,
  pointFill: '#ffb347',
  goodLine: '#30ffd8',
  badLine: '#ff6b6b',
}

const LANDMARK_EDGES: Array<[number, number]> = [
  [11, 12],
  [11, 13],
  [13, 15],
  [15, 17],
  [17, 19],
  [19, 21],
  [21, 15],
  [12, 14],
  [14, 16],
  [16, 18],
  [18, 20],
  [20, 22],
  [22, 16],
  [20, 16],
  [11, 23],
  [12, 24],
  [23, 24],
  [23, 25],
  [25, 27],
  [24, 26],
  [26, 28],
  [27, 29],
  [29, 31],
  [31, 27],
  [28, 30],
  [30, 32],
  [32, 28],
]

export function VideoStage({
  videoUrl,
  strokes,
  landmarkFrames,
  landmarkFps,
  trackedLines,
  frameAnalysis,
}: VideoStageProps) {
  const videoRef = useRef<HTMLVideoElement>(null)
  const canvasRef = useRef<HTMLCanvasElement>(null)
  const lastFrameRef = useRef<LandmarkFrame | null>(null)
  const lastAnalysisRef = useRef<FrameAnalysis | null>(null)
  const [dimensions, setDimensions] = useState({ width: 0, height: 0 })

  const syncCanvasSize = useCallback(() => {
    const videoEl = videoRef.current
    const canvasEl = canvasRef.current
    if (!videoEl || !canvasEl) {
      return
    }

    const { width, height } = videoEl.getBoundingClientRect()
    if (!width || !height) {
      return
    }

    const nextWidth = Math.round(width)
    const nextHeight = Math.round(height)
    if (canvasEl.width !== nextWidth) {
      canvasEl.width = nextWidth
    }
    if (canvasEl.height !== nextHeight) {
      canvasEl.height = nextHeight
    }
    setDimensions({ width: nextWidth, height: nextHeight })
  }, [])

  useEffect(() => {
    if (!videoUrl) {
      setDimensions({ width: 0, height: 0 })
      return
    }
    syncCanvasSize()
  }, [syncCanvasSize, videoUrl])

  useEffect(() => {
    lastFrameRef.current = null
    lastAnalysisRef.current = null
  }, [videoUrl, landmarkFrames, frameAnalysis])

  useEffect(() => {
    const videoEl = videoRef.current
    if (!videoEl) {
      return
    }

    if (typeof ResizeObserver !== 'undefined') {
      const observer = new ResizeObserver(() => syncCanvasSize())
      observer.observe(videoEl)
      return () => observer.disconnect()
    }

    window.addEventListener('resize', syncCanvasSize)
    return () => window.removeEventListener('resize', syncCanvasSize)
  }, [syncCanvasSize, videoUrl])

  const drawFrame = useCallback(() => {
    const canvas = canvasRef.current
    const ctx = canvas?.getContext('2d')
    const videoEl = videoRef.current
    if (!canvas || !ctx) {
      return
    }

    ctx.clearRect(0, 0, canvas.width, canvas.height)
    if (!videoUrl || !dimensions.width || !dimensions.height) {
      return
    }

    if (strokes.length > 0) {
      ctx.strokeStyle = DRAW_STYLE.strokeStyle
      ctx.lineWidth = DRAW_STYLE.lineWidth
      ctx.lineJoin = 'round'
      ctx.lineCap = 'round'

      strokes.forEach((stroke) => {
        const points = stroke.points.map(([x, y]) => [x * dimensions.width, y * dimensions.height])
        ctx.beginPath()
        points.forEach(([px, py], index) => {
          if (index === 0) {
            ctx.moveTo(px, py)
          } else {
            ctx.lineTo(px, py)
          }
        })
        ctx.stroke()
      })
    }

    if (!videoEl || !landmarkFrames || landmarkFrames.length === 0) {
      return
    }

    const duration = videoEl.duration || 0
    const fps = typeof landmarkFps === 'number' && landmarkFps > 0 ? landmarkFps : 0
    const safeIndex = fps > 0
      ? Math.min(landmarkFrames.length - 1, Math.floor(videoEl.currentTime * fps))
      : duration > 0
        ? Math.min(landmarkFrames.length - 1, Math.floor((videoEl.currentTime / duration) * landmarkFrames.length))
        : 0
    const rawFrame = landmarkFrames[safeIndex]
    const frame = rawFrame && rawFrame.length > 0 ? rawFrame : lastFrameRef.current
    if (!frame || frame.length === 0) {
      return
    }

    const hasPixelCoords = frame.some((point) => point.x > 1 || point.y > 1)
    const sourceWidth = videoEl.videoWidth || dimensions.width
    const sourceHeight = videoEl.videoHeight || dimensions.height
    const scale = Math.min(dimensions.width / sourceWidth, dimensions.height / sourceHeight)
    const contentWidth = sourceWidth * scale
    const contentHeight = sourceHeight * scale
    const offsetX = (dimensions.width - contentWidth) / 2
    const offsetY = (dimensions.height - contentHeight) / 2
    const pointRadius = Math.max(1.5, Math.min(4, Math.round(dimensions.width * 0.004)))

    ctx.fillStyle = DRAW_STYLE.pointFill
    const pointCoords = frame.map((point) => {
      const px = hasPixelCoords
        ? (point.x / sourceWidth) * contentWidth + offsetX
        : point.x * contentWidth + offsetX
      const py = hasPixelCoords
        ? (point.y / sourceHeight) * contentHeight + offsetY
        : point.y * contentHeight + offsetY
      return [px, py] as const
    })

    pointCoords.forEach(([px, py]) => {
      ctx.beginPath()
      ctx.arc(px, py, pointRadius, 0, Math.PI * 2)
      ctx.fill()
    })

    let rawAnalysis: FrameAnalysis | undefined
    let analysisEntry: FrameAnalysis | null = null

    if (pointCoords.length > 0) {
      rawAnalysis = frameAnalysis?.[safeIndex]
        ?? frameAnalysis?.find((entry) => entry.frame_index === safeIndex)
      analysisEntry = rawFrame && rawFrame.length > 0 ? rawAnalysis ?? null : lastAnalysisRef.current

      const goodLines = analysisEntry?.green_lines
      const badLines = analysisEntry?.red_lines
      const fallbackLines = trackedLines?.length ? trackedLines : LANDMARK_EDGES.map(([start, end]) => ({ start, end }))

      const drawLineList = (lines: LineSegment[], color: string) => {
        ctx.strokeStyle = color
        ctx.lineWidth = Math.max(1, Math.round(pointRadius * 0.7))
        ctx.lineJoin = 'round'
        ctx.lineCap = 'round'

        lines.forEach(({ start, end }) => {
          const startPoint = pointCoords[start]
          const endPoint = pointCoords[end]
          if (!startPoint || !endPoint) {
            return
          }
          ctx.beginPath()
          ctx.moveTo(startPoint[0], startPoint[1])
          ctx.lineTo(endPoint[0], endPoint[1])
          ctx.stroke()
        })
      }

      if (badLines && badLines.length > 0) {
        drawLineList(badLines, DRAW_STYLE.badLine)
      }
      if (goodLines && goodLines.length > 0) {
        drawLineList(goodLines, DRAW_STYLE.goodLine)
      }
      if ((!badLines || badLines.length === 0) && (!goodLines || goodLines.length === 0)) {
        drawLineList(fallbackLines, DRAW_STYLE.pointFill)
      }
    }

    lastFrameRef.current = frame
    if (rawFrame && rawFrame.length > 0 && rawAnalysis) {
      lastAnalysisRef.current = rawAnalysis
    }
  }, [dimensions, frameAnalysis, landmarkFrames, strokes, trackedLines, videoUrl])

  useEffect(() => {
    let animationId = 0

    const render = () => {
      drawFrame()
      if (landmarkFrames && landmarkFrames.length > 0) {
        animationId = window.requestAnimationFrame(render)
      }
    }

    render()

    return () => {
      if (animationId) {
        window.cancelAnimationFrame(animationId)
      }
    }
  }, [drawFrame, landmarkFrames])

  if (!videoUrl) {
    return (
      <div className="placeholder">
        <p>Your uploaded video will appear here.</p>
        <span>Use the panel to the left to get started.</span>
      </div>
    )
  }

  return (
    <div className="video-stage">
      <video
        ref={videoRef}
        src={videoUrl}
        controls
        playsInline
        onLoadedMetadata={syncCanvasSize}
        className="video-player"
      />
      <canvas ref={canvasRef} className="overlay-canvas" />
    </div>
  )
}
