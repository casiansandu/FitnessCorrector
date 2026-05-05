def evaluate_frame(landmarks, pose_landmark, calculate_angle):
    failed_checks = []
    red_lines = set()

    left_elbow_angle = calculate_angle(
        landmarks[pose_landmark.LEFT_SHOULDER],
        landmarks[pose_landmark.LEFT_ELBOW],
        landmarks[pose_landmark.LEFT_WRIST],
    )
    right_elbow_angle = calculate_angle(
        landmarks[pose_landmark.RIGHT_SHOULDER],
        landmarks[pose_landmark.RIGHT_ELBOW],
        landmarks[pose_landmark.RIGHT_WRIST],
    )

    avg_elbow_angle = (left_elbow_angle + right_elbow_angle) / 2

    left_shoulder_angle = calculate_angle(
        landmarks[pose_landmark.LEFT_HIP],
        landmarks[pose_landmark.LEFT_SHOULDER],
        landmarks[pose_landmark.LEFT_ELBOW],
    )
    right_shoulder_angle = calculate_angle(
        landmarks[pose_landmark.RIGHT_HIP],
        landmarks[pose_landmark.RIGHT_SHOULDER],
        landmarks[pose_landmark.RIGHT_ELBOW],
    )
    avg_shoulder_angle = (left_shoulder_angle + right_shoulder_angle) / 2

    if avg_elbow_angle < 55.0:
        failed_checks.append("bench_press: elbows tucked too deep below torso")
        red_lines.update(
            [
                (pose_landmark.LEFT_SHOULDER, pose_landmark.LEFT_ELBOW),
                (pose_landmark.RIGHT_SHOULDER, pose_landmark.RIGHT_ELBOW),
            ]
        )

    if avg_shoulder_angle > 120.0:
        failed_checks.append("bench_press: elbows flared wide")
        red_lines.update(
            [
                (pose_landmark.LEFT_SHOULDER, pose_landmark.LEFT_ELBOW),
                (pose_landmark.RIGHT_SHOULDER, pose_landmark.RIGHT_ELBOW),
                (pose_landmark.LEFT_SHOULDER, pose_landmark.LEFT_HIP),
                (pose_landmark.RIGHT_SHOULDER, pose_landmark.RIGHT_HIP),
            ]
        )

    return {
        "failed_checks": failed_checks,
        "red_lines": list(red_lines),
    }
