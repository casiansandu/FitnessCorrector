def evaluate_frame(landmarks, pose_landmark, calculate_angle):
    failed_checks = []
    red_lines = set()

    left_shoulder_y = landmarks[pose_landmark.LEFT_SHOULDER].y
    right_shoulder_y = landmarks[pose_landmark.RIGHT_SHOULDER].y
    left_hip_y = landmarks[pose_landmark.LEFT_HIP].y
    right_hip_y = landmarks[pose_landmark.RIGHT_HIP].y

    avg_shoulder_y = (left_shoulder_y + right_shoulder_y) / 2
    avg_hip_y = (left_hip_y + right_hip_y) / 2

    left_hip_angle = calculate_angle(
        landmarks[pose_landmark.LEFT_SHOULDER],
        landmarks[pose_landmark.LEFT_HIP],
        landmarks[pose_landmark.LEFT_KNEE],
    )
    right_hip_angle = calculate_angle(
        landmarks[pose_landmark.RIGHT_SHOULDER],
        landmarks[pose_landmark.RIGHT_HIP],
        landmarks[pose_landmark.RIGHT_KNEE],
    )
    avg_hip_angle = (left_hip_angle + right_hip_angle) / 2

    if avg_shoulder_y > avg_hip_y + 0.05:
        failed_checks.append("deadlift_start: shoulders are drifting below hips")
        red_lines.update(
            [
                (pose_landmark.LEFT_SHOULDER, pose_landmark.LEFT_HIP),
                (pose_landmark.RIGHT_SHOULDER, pose_landmark.RIGHT_HIP),
            ]
        )

    if avg_hip_angle < 150.0:
        failed_checks.append("deadlift_back: hips are collapsing (rounded back)")
        red_lines.update(
            [
                (pose_landmark.LEFT_SHOULDER, pose_landmark.LEFT_HIP),
                (pose_landmark.LEFT_HIP, pose_landmark.LEFT_KNEE),
                (pose_landmark.RIGHT_SHOULDER, pose_landmark.RIGHT_HIP),
                (pose_landmark.RIGHT_HIP, pose_landmark.RIGHT_KNEE),
            ]
        )

    return {
        "failed_checks": failed_checks,
        "red_lines": list(red_lines),
    }
