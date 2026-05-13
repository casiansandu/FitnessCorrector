def evaluate_frame(landmarks, pose_landmark, calculate_angle):
    failed_checks = []
    red_lines = set()

    avg_shoulder_y = (landmarks[pose_landmark.LEFT_SHOULDER].y + landmarks[pose_landmark.RIGHT_SHOULDER].y) / 2
    avg_hip_y = (landmarks[pose_landmark.LEFT_HIP].y + landmarks[pose_landmark.RIGHT_HIP].y) / 2

    if avg_hip_y > avg_shoulder_y + 0.08:
        failed_checks.append("push_up: hips sagging below shoulders")
        red_lines.update(
            [
                (pose_landmark.LEFT_SHOULDER, pose_landmark.LEFT_HIP),
                (pose_landmark.RIGHT_SHOULDER, pose_landmark.RIGHT_HIP),
            ]
        )

    return {
        "failed_checks": failed_checks,
        "red_lines": list(red_lines),
    }
