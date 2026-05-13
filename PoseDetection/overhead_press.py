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

    avg_wrist_y = (landmarks[pose_landmark.LEFT_WRIST].y + landmarks[pose_landmark.RIGHT_WRIST].y) / 2
    avg_shoulder_y = (landmarks[pose_landmark.LEFT_SHOULDER].y + landmarks[pose_landmark.RIGHT_SHOULDER].y) / 2
    wrists_overhead = avg_wrist_y < avg_shoulder_y - 0.05

    if wrists_overhead and avg_elbow_angle < 150.0:
        failed_checks.append("overhead_press: lockout missing (elbows still bent overhead)")
        red_lines.update(
            [
                (pose_landmark.LEFT_SHOULDER, pose_landmark.LEFT_ELBOW),
                (pose_landmark.RIGHT_SHOULDER, pose_landmark.RIGHT_ELBOW),
            ]
        )

    if wrists_overhead and avg_shoulder_angle > 135.0:
        failed_checks.append("overhead_press: elbows flared wide overhead")
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
