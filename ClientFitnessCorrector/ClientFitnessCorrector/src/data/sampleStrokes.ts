import type { PoseStroke } from '../types/pose'

export const SAMPLE_STROKES: PoseStroke[] = [
  {
    id: 'spine',
    points: [
      [0.5, 0.08],
      [0.5, 0.8],
    ],
  },
  {
    id: 'left-arm',
    points: [
      [0.5, 0.32],
      [0.34, 0.22],
      [0.24, 0.38],
    ],
  },
  {
    id: 'right-arm',
    points: [
      [0.5, 0.32],
      [0.66, 0.2],
      [0.74, 0.38],
    ],
  },
  {
    id: 'left-leg',
    points: [
      [0.5, 0.8],
      [0.44, 0.92],
      [0.4, 1],
    ],
  },
  {
    id: 'right-leg',
    points: [
      [0.5, 0.8],
      [0.56, 0.92],
      [0.6, 1],
    ],
  },
]
