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

    avg_wrist_y = (landmarks[pose_landmark.LEFT_WRIST].y + landmarks[pose_landmark.RIGHT_WRIST].y) / 2
    avg_elbow_y = (landmarks[pose_landmark.LEFT_ELBOW].y + landmarks[pose_landmark.RIGHT_ELBOW].y) / 2
    wrist_above_elbow = avg_wrist_y < avg_elbow_y - 0.02

    if wrist_above_elbow and avg_elbow_angle > 90.0:
        failed_checks.append("bicep_curl: limited flexion at the top of the curl")
        red_lines.update(
            [
                (pose_landmark.LEFT_ELBOW, pose_landmark.LEFT_WRIST),
                (pose_landmark.RIGHT_ELBOW, pose_landmark.RIGHT_WRIST),
            ]
        )

    return {
        "failed_checks": failed_checks,
        "red_lines": list(red_lines),
    }
