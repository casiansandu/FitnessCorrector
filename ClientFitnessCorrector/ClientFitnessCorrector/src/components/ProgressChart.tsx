type ProgressChartPoint = {
  date: string
  value: number
}

type ProgressChartProps = {
  title: string
  unit?: string
  yLabel?: string
  xLabel?: string
  points: ProgressChartPoint[]
}

export function ProgressChart({ title, unit, yLabel, xLabel, points }: ProgressChartProps) {
  if (points.length === 0) {
    return (
      <div className="progress-chart">
        <div className="progress-chart__header">
          <h3>{title}</h3>
        </div>
        <div className="progress-chart__empty">No data yet. Complete another session.</div>
      </div>
    )
  }

  const values = points.map((point) => point.value)
  const minValue = Math.min(...values)
  const maxValue = Math.max(...values)
  const range = maxValue - minValue || 1
  const padding = 16
  const height = 180
  const width = 520

  const toX = (index: number) => padding + (index / (points.length - 1 || 1)) * (width - padding * 2)
  const toY = (value: number) => padding + (1 - (value - minValue) / range) * (height - padding * 2)

  const linePath = points
    .map((point, index) => `${index === 0 ? 'M' : 'L'} ${toX(index)} ${toY(point.value)}`)
    .join(' ')

  return (
    <div className="progress-chart">
      <div className="progress-chart__header">
        <h3>{title}</h3>
        <span>{unit ? unit : ''}</span>
      </div>
      <svg viewBox={`0 0 ${width} ${height}`} role="img" aria-label={`${title} trend`}>
        <path className="progress-chart__path" d={linePath} />
        {points.map((point, index) => (
          <circle
            key={`${point.date}-${index}`}
            className="progress-chart__dot"
            cx={toX(index)}
            cy={toY(point.value)}
            r={4}
          >
            <title>
              {point.date} - {point.value.toFixed(2)}{unit ? ` ${unit}` : ''}
            </title>
          </circle>
        ))}
      </svg>
      <div className="progress-chart__axis">
        <span>{yLabel ?? ''}</span>
        <span>{xLabel ?? ''}</span>
      </div>
      <div className="progress-chart__footer">
        <span>{points[0].date}</span>
        <span>
          {minValue.toFixed(2)} - {maxValue.toFixed(2)}
        </span>
        <span>{points[points.length - 1].date}</span>
      </div>
    </div>
  )
}
