def evaluate_frame(landmarks, pose_landmark, calculate_angle):
    failed_checks = []
    red_lines = set()

    left_knee_y = landmarks[pose_landmark.LEFT_KNEE].y
    right_knee_y = landmarks[pose_landmark.RIGHT_KNEE].y
    left_hip_y = landmarks[pose_landmark.LEFT_HIP].y
    right_hip_y = landmarks[pose_landmark.RIGHT_HIP].y

    left_knee_angle = calculate_angle(
        landmarks[pose_landmark.LEFT_HIP],
        landmarks[pose_landmark.LEFT_KNEE],
        landmarks[pose_landmark.LEFT_ANKLE],
    )
    right_knee_angle = calculate_angle(
        landmarks[pose_landmark.RIGHT_HIP],
        landmarks[pose_landmark.RIGHT_KNEE],
        landmarks[pose_landmark.RIGHT_ANKLE],
    )

    avg_knee_y = (left_knee_y + right_knee_y) / 2
    avg_hip_y = (left_hip_y + right_hip_y) / 2
    avg_knee_angle = (left_knee_angle + right_knee_angle) / 2

    # Only score squat depth while the knees are flexed.
    is_in_squat_phase = avg_knee_angle < 155.0

    if is_in_squat_phase and avg_hip_y < (avg_knee_y - 0.04):
        failed_checks.append(
            "squat_depth: hips stay high relative to knees (shallow squat frame)"
        )
        red_lines.update(
            [
                (pose_landmark.LEFT_HIP, pose_landmark.LEFT_KNEE),
                (pose_landmark.RIGHT_HIP, pose_landmark.RIGHT_KNEE),
            ]
        )

    return {
        "failed_checks": failed_checks,
        "red_lines": list(red_lines),
    }
