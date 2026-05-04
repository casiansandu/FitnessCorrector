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


def normalize_exercise_name(exercise_name):
    return exercise_name.strip().lower().replace('-', '').replace('_', '').replace(' ', '')


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
    base_options = python.BaseOptions(model_asset_path='./pose_landmarker_heavy.task')
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


def process_video_frames(cap, detector, fps, exercise_evaluator):
    frame_index = 0
    total_frames_analyzed = 0
    frames_with_poor_form = 0
    landmark_frames = []
    frame_analysis = []

    while cap.isOpened():
        ret, frame = cap.read()
        if not ret:
            break

        rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=rgb_frame)
        timestamp_ms = int((frame_index / fps) * 1000)

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

    return {
        "total_frames_analyzed": total_frames_analyzed,
        "frames_with_poor_form": frames_with_poor_form,
        "landmark_frames": landmark_frames,
        "frame_analysis": frame_analysis,
    }

def main():
    workout_session_id, video_path, exercise_name, output_path = parse_cli_args()
    exercise_evaluator = load_exercise_evaluator(exercise_name)
    if exercise_evaluator is None:
        print(f"Info: No plugin found for exercise slug '{exercise_name}'. Using generic checks only.")

    detector = create_pose_detector()

    cap = cv2.VideoCapture(video_path)
    fps = get_video_fps(cap)
    results = process_video_frames(cap, detector, fps, exercise_evaluator)

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