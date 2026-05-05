import sys
import os
import json
import importlib.util
import cv2
import numpy as np
import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision
from landmarksEnum import PoseLandmark

# 1. Hide TensorFlow C++ warnings so they don't mess up the C# string output
os.environ['TF_CPP_MIN_LOG_LEVEL'] = '3'

# Lines that can be drawn by the frontend/server as green/red according to frame analysis.
TRACKED_LINES = [
    (PoseLandmark.LEFT_SHOULDER, PoseLandmark.RIGHT_SHOULDER),
    (PoseLandmark.LEFT_SHOULDER, PoseLandmark.LEFT_ELBOW),
    (PoseLandmark.LEFT_ELBOW, PoseLandmark.LEFT_WRIST),
    (PoseLandmark.RIGHT_SHOULDER, PoseLandmark.RIGHT_ELBOW),
    (PoseLandmark.RIGHT_ELBOW, PoseLandmark.RIGHT_WRIST),
    (PoseLandmark.LEFT_SHOULDER, PoseLandmark.LEFT_HIP),
    (PoseLandmark.RIGHT_SHOULDER, PoseLandmark.RIGHT_HIP),
    (PoseLandmark.LEFT_HIP, PoseLandmark.RIGHT_HIP),
    (PoseLandmark.LEFT_HIP, PoseLandmark.LEFT_KNEE),
    (PoseLandmark.LEFT_KNEE, PoseLandmark.LEFT_ANKLE),
    (PoseLandmark.RIGHT_HIP, PoseLandmark.RIGHT_KNEE),
    (PoseLandmark.RIGHT_KNEE, PoseLandmark.RIGHT_ANKLE),
]

MODEL_LITE_PATH = "./pose_landmarker_lite.task"
MODEL_HEAVY_PATH = "./pose_landmarker_heavy.task"
MAX_ANALYSIS_SECONDS = 20.0
FRAME_STRIDE = 2
TARGET_FRAME_WIDTH = 640

EXERCISE_METRICS_CONFIG = {
    "squat": {
        "left": (PoseLandmark.LEFT_HIP, PoseLandmark.LEFT_KNEE, PoseLandmark.LEFT_ANKLE),
        "right": (PoseLandmark.RIGHT_HIP, PoseLandmark.RIGHT_KNEE, PoseLandmark.RIGHT_ANKLE),
        "min_range_ratio": 0.25,
    },
    "deadlift": {
        "left": (PoseLandmark.LEFT_SHOULDER, PoseLandmark.LEFT_HIP, PoseLandmark.LEFT_KNEE),
        "right": (PoseLandmark.RIGHT_SHOULDER, PoseLandmark.RIGHT_HIP, PoseLandmark.RIGHT_KNEE),
        "min_range_ratio": 0.2,
    },
    "benchpress": {
        "left": (PoseLandmark.LEFT_SHOULDER, PoseLandmark.LEFT_ELBOW, PoseLandmark.LEFT_WRIST),
        "right": (PoseLandmark.RIGHT_SHOULDER, PoseLandmark.RIGHT_ELBOW, PoseLandmark.RIGHT_WRIST),
        "min_range_ratio": 0.2,
    },
}


def normalize_exercise_name(exercise_name):
    return exercise_name.strip().lower().replace('-', '').replace('_', '').replace(' ', '')


def get_metrics_config(exercise_name):
    normalized = normalize_exercise_name(exercise_name)
    config = EXERCISE_METRICS_CONFIG.get(normalized)
    if config is not None:
        return config

    if "squat" in normalized:
        return EXERCISE_METRICS_CONFIG.get("squat")
    if "deadlift" in normalized:
        return EXERCISE_METRICS_CONFIG.get("deadlift")
    if "bench" in normalized:
        return EXERCISE_METRICS_CONFIG.get("benchpress")

    return None


def line_to_dict(line):
    return {"start": int(line[0]), "end": int(line[1])}


def calculate_angle(a, b, c):
    ba = np.array([a.x - b.x, a.y - b.y])
    bc = np.array([c.x - b.x, c.y - b.y])

    denominator = np.linalg.norm(ba) * np.linalg.norm(bc)
    if denominator == 0:
        return 180.0

    cosine = np.dot(ba, bc) / denominator
    cosine = np.clip(cosine, -1.0, 1.0)
    return float(np.degrees(np.arccos(cosine)))


def clamp(value, min_value, max_value):
    return max(min_value, min(max_value, value))


def smooth_series(values, window=5):
    if window <= 1 or len(values) < 3:
        return values
    half = window // 2
    smoothed = []
    for i in range(len(values)):
        start = max(0, i - half)
        end = min(len(values), i + half + 1)
        smoothed.append(sum(values[start:end]) / (end - start))
    return smoothed


def find_local_extrema(values, kind, threshold):
    indices = []
    for i in range(1, len(values) - 1):
        if kind == "min" and values[i] < values[i - 1] and values[i] < values[i + 1] and values[i] <= threshold:
            indices.append(i)
        if kind == "max" and values[i] > values[i - 1] and values[i] > values[i + 1] and values[i] >= threshold:
            indices.append(i)
    return indices


def safe_average(values):
    return sum(values) / len(values) if values else 0.0


def build_rep_metrics(samples, min_range_ratio):
    if len(samples) < 3:
        return [], 0.0, 0.0, 0.0

    angles = [sample["avg_angle"] for sample in samples]
    smoothed = smooth_series(angles, window=5)
    max_angle = max(smoothed)
    min_angle = min(smoothed)
    angle_range = max_angle - min_angle
    if angle_range < 5:
        return [], angle_range, max_angle, min_angle

    min_threshold = max_angle - (angle_range * min_range_ratio)
    max_threshold = min_angle + (angle_range * min_range_ratio)

    minima = find_local_extrema(smoothed, "min", min_threshold)
    maxima = find_local_extrema(smoothed, "max", max_threshold)

    reps = []
    for rep_index, min_idx in enumerate(minima):
        prev_max = max([idx for idx in maxima if idx < min_idx], default=None)
        next_max = min([idx for idx in maxima if idx > min_idx], default=None)
        if prev_max is None or next_max is None:
            continue

        start_time = samples[prev_max]["timestamp_ms"]
        end_time = samples[next_max]["timestamp_ms"]
        mid_time = samples[min_idx]["timestamp_ms"]

        total_seconds = max(0.0, (end_time - start_time) / 1000.0)
        eccentric_seconds = max(0.0, (mid_time - start_time) / 1000.0)
        concentric_seconds = max(0.0, (end_time - mid_time) / 1000.0)

        depth = clamp((smoothed[prev_max] - smoothed[min_idx]) / 180.0, 0.0, 1.0)

        diff_values = [sample["symmetry_diff"] for sample in samples[prev_max:next_max + 1]]
        symmetry = clamp(1.0 - (safe_average(diff_values) / 180.0), 0.0, 1.0)

        reps.append({
            "rep_index": rep_index,
            "depth": depth,
            "tempo_total_seconds": total_seconds,
            "tempo_eccentric_seconds": eccentric_seconds,
            "tempo_concentric_seconds": concentric_seconds,
            "symmetry": symmetry,
        })

    return reps, angle_range, max_angle, min_angle


def summarize_metrics(samples, min_range_ratio):
    reps, angle_range, max_angle, min_angle = build_rep_metrics(samples, min_range_ratio)
    if reps:
        avg_depth = safe_average([rep["depth"] for rep in reps])
        avg_tempo = safe_average([rep["tempo_total_seconds"] for rep in reps])
        avg_symmetry = safe_average([rep["symmetry"] for rep in reps])
        rep_count = len(reps)
    else:
        avg_depth = clamp(angle_range / 180.0, 0.0, 1.0)
        avg_symmetry = clamp(1.0 - (safe_average([sample["symmetry_diff"] for sample in samples]) / 180.0), 0.0, 1.0)
        avg_tempo = 0.0
        rep_count = 0

    return {
        "session": {
            "avg_depth": round(avg_depth, 4),
            "avg_tempo_seconds": round(avg_tempo, 4),
            "avg_symmetry": round(avg_symmetry, 4),
            "rep_count": rep_count,
        },
        "reps": reps,
    }


def load_exercise_evaluator(exercise_name):
    slug = exercise_name.strip().lower()
    module_dir = os.path.dirname(__file__)

    candidate_files = []
    direct_slug = f"{slug}.py"
    slug_with_underscores = f"{slug.replace('-', '_').replace(' ', '_')}.py"
    normalized_slug = f"{normalize_exercise_name(exercise_name)}.py"

    for file_name in [direct_slug, slug_with_underscores, normalized_slug]:
        if file_name not in candidate_files:
            candidate_files.append(file_name)

    for file_name in candidate_files:
        module_path = os.path.join(module_dir, file_name)
        if not os.path.exists(module_path):
            continue

        module_name = f"exercise_{os.path.splitext(file_name)[0]}"
        spec = importlib.util.spec_from_file_location(module_name, module_path)
        if spec is None or spec.loader is None:
            continue

        module = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(module)
        evaluator = getattr(module, "evaluate_frame", None)
        if callable(evaluator):
            return evaluator

    return None


def evaluate_form_for_frame(landmarks, exercise_evaluator=None):

    red_lines = set()
    failed_checks = []

    left_ear_y = landmarks[PoseLandmark.LEFT_EAR].y
    right_ear_y = landmarks[PoseLandmark.RIGHT_EAR].y
    left_shoulder_y = landmarks[PoseLandmark.LEFT_SHOULDER].y
    right_shoulder_y = landmarks[PoseLandmark.RIGHT_SHOULDER].y

    avg_ear_y = (left_ear_y + right_ear_y) / 2
    avg_shoulder_y = (left_shoulder_y + right_shoulder_y) / 2

    # Generic check used for all exercises until a specific exercise plugin is added.
    if avg_ear_y > avg_shoulder_y:
        failed_checks.append(
            "head_drop: ears are lower than shoulders (likely rounded/slouched upper back)"
        )
        red_lines.update(
            [
                (PoseLandmark.LEFT_SHOULDER, PoseLandmark.RIGHT_SHOULDER),
                (PoseLandmark.LEFT_SHOULDER, PoseLandmark.LEFT_ELBOW),
                (PoseLandmark.RIGHT_SHOULDER, PoseLandmark.RIGHT_ELBOW),
            ]
        )

    if callable(exercise_evaluator):
        exercise_result = exercise_evaluator(landmarks, PoseLandmark, calculate_angle)
        plugin_failed_checks = exercise_result.get("failed_checks", [])
        plugin_red_lines = exercise_result.get("red_lines", [])

        failed_checks.extend(str(check) for check in plugin_failed_checks)
        for line in plugin_red_lines:
            if isinstance(line, (tuple, list)) and len(line) == 2:
                red_lines.add((int(line[0]), int(line[1])))

    all_lines = set(TRACKED_LINES)
    green_lines = all_lines - red_lines

    return {
        "bad_form": len(failed_checks) > 0,
        "failed_checks": failed_checks,
        "red_lines": [line_to_dict(line) for line in sorted(red_lines)],
        "green_lines": [line_to_dict(line) for line in sorted(green_lines)],
    }


def parse_cli_args():
    if len(sys.argv) < 5:
        print("Error: Insufficient arguments provided by .NET.")
        sys.exit(1)

    workout_session_id = sys.argv[1]
    video_path = sys.argv[2]
    exercise_name = sys.argv[3]
    output_path = sys.argv[4]

    if not os.path.exists(video_path):
        print(f"Error: Video file not found at {video_path}")
        sys.exit(1)

    return workout_session_id, video_path, exercise_name, output_path


def create_pose_detector():
    model_path = MODEL_LITE_PATH if os.path.exists(MODEL_LITE_PATH) else MODEL_HEAVY_PATH
    base_options = python.BaseOptions(model_asset_path=model_path)
    options = vision.PoseLandmarkerOptions(
        base_options=base_options,
        running_mode=vision.RunningMode.VIDEO, # Tell it we are feeding a video stream
        output_segmentation_masks=False # Turned off to save processing power on the server
    )
    return vision.PoseLandmarker.create_from_options(options)


def get_video_fps(cap):
    fps = cap.get(cv2.CAP_PROP_FPS)
    if fps == 0:
        return 30
    return fps


def process_video_frames(cap, detector, fps, exercise_evaluator, metrics_config):
    frame_index = 0
    total_frames_analyzed = 0
    frames_with_poor_form = 0
    landmark_frames = []
    frame_analysis = []
    metric_samples = []

    while cap.isOpened():
        ret, frame = cap.read()
        if not ret:
            break

        elapsed_seconds = frame_index / fps
        if elapsed_seconds > MAX_ANALYSIS_SECONDS:
            break

        if frame.shape[1] > TARGET_FRAME_WIDTH:
            scale = TARGET_FRAME_WIDTH / frame.shape[1]
            frame = cv2.resize(frame, (TARGET_FRAME_WIDTH, int(frame.shape[0] * scale)))

        rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=rgb_frame)
        timestamp_ms = int((frame_index / fps) * 1000)

        if frame_index % FRAME_STRIDE != 0:
            landmark_frames.append([])
            frame_analysis.append(
                {
                    "frame_index": frame_index,
                    "timestamp_ms": timestamp_ms,
                    "bad_form": None,
                    "failed_checks": ["skipped_frame"],
                    "red_lines": [],
                    "green_lines": [],
                }
            )
            frame_index += 1
            continue

        detection_result = detector.detect_for_video(mp_image, timestamp_ms)
        vision.PoseLandmarkerResult

        frame_landmarks = []
        if detection_result.pose_landmarks:
            primary_pose = detection_result.pose_landmarks[0]
            frame_landmarks = [
                {
                    "x": round(lm.x, 3),
                    "y": round(lm.y, 3),
                    "z": round(lm.z, 3)
                }
                for lm in primary_pose
            ]
        landmark_frames.append(frame_landmarks)

        if detection_result.pose_landmarks:
            landmarks = detection_result.pose_landmarks[0]
            total_frames_analyzed += 1

            if metrics_config is not None:
                left_triplet = metrics_config["left"]
                right_triplet = metrics_config["right"]
                left_angle = calculate_angle(
                    landmarks[left_triplet[0]],
                    landmarks[left_triplet[1]],
                    landmarks[left_triplet[2]],
                )
                right_angle = calculate_angle(
                    landmarks[right_triplet[0]],
                    landmarks[right_triplet[1]],
                    landmarks[right_triplet[2]],
                )
                avg_angle = (left_angle + right_angle) / 2
                metric_samples.append({
                    "timestamp_ms": timestamp_ms,
                    "left_angle": left_angle,
                    "right_angle": right_angle,
                    "avg_angle": avg_angle,
                    "symmetry_diff": abs(left_angle - right_angle),
                })

            per_frame_form = evaluate_form_for_frame(landmarks, exercise_evaluator)
            frame_analysis.append(
                {
                    "frame_index": frame_index,
                    "timestamp_ms": timestamp_ms,
                    **per_frame_form,
                }
            )

            if per_frame_form["bad_form"]:
                frames_with_poor_form += 1
        else:
            frame_analysis.append(
                {
                    "frame_index": frame_index,
                    "timestamp_ms": timestamp_ms,
                    "bad_form": None,
                    "failed_checks": ["no_pose_detected"],
                    "red_lines": [],
                    "green_lines": [],
                }
            )

        frame_index += 1

    metrics = None
    if metrics_config is not None:
        metrics = summarize_metrics(metric_samples, metrics_config["min_range_ratio"])

    return {
        "total_frames_analyzed": total_frames_analyzed,
        "frames_with_poor_form": frames_with_poor_form,
        "landmark_frames": landmark_frames,
        "frame_analysis": frame_analysis,
        "metrics": metrics,
    }

def main():
    workout_session_id, video_path, exercise_name, output_path = parse_cli_args()
    exercise_evaluator = load_exercise_evaluator(exercise_name)
    metrics_config = get_metrics_config(exercise_name)
    if exercise_evaluator is None:
        print(f"Info: No plugin found for exercise slug '{exercise_name}'. Using generic checks only.")

    detector = create_pose_detector()

    cap = cv2.VideoCapture(video_path)
    fps = get_video_fps(cap)
    results = process_video_frames(cap, detector, fps, exercise_evaluator, metrics_config)

    cap.release()
    detector.close()

    if results["total_frames_analyzed"] == 0:
        print("Analysis Failed: No person detected in the video.")
        sys.exit(1)

    mistake_percentage = (
        results["frames_with_poor_form"] / results["total_frames_analyzed"]
    ) * 100

    output_dir = os.path.join(os.path.dirname(__file__), 'PoseDetectionResults')
    os.makedirs(output_dir, exist_ok=True)
    payload = {
        "workout_session_id": workout_session_id,
        "exercise_name": exercise_name,
        "fps": fps,
        "landmarks": results["landmark_frames"],
        "tracked_lines": [line_to_dict(line) for line in TRACKED_LINES],
        "frame_analysis": results["frame_analysis"],
        "metrics": results.get("metrics"),
        "summary": {
            "total_frames_analyzed": results["total_frames_analyzed"],
            "frames_with_poor_form": results["frames_with_poor_form"],
            "mistake_percentage": round(mistake_percentage, 2),
            "bad_form_threshold_percent": 20.0,
        },
    }
    with open(output_path, 'w', encoding='utf-8') as output_file:
        json.dump(payload, output_file, indent=2)

    # This exact string is what gets saved to your WorkoutSession.AiFeedback in the Database!
    if mistake_percentage > 20:
        print(f"Poor Form: You dropped your head in {mistake_percentage:.1f}% of the movement. Keep your chest up.")
    else:
        print(f"Great Job: Form was solid. Deviations detected in only {mistake_percentage:.1f}% of frames.")
    
    
if __name__ == "__main__":
    main()