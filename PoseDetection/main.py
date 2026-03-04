import sys
import os
import cv2
import numpy as np
import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision

# 1. Hide TensorFlow C++ warnings so they don't mess up the C# string output
os.environ['TF_CPP_MIN_LOG_LEVEL'] = '3' 

def main():
    # 2. Grab the video path passed by the .NET Controller
    if len(sys.argv) < 3:
        print("Error: No video path or exercise name provided by .NET.")
        sys.exit(1)

    video_path = sys.argv[1]
    exercise_name = sys.argv[2]

    if not os.path.exists(video_path):
        print(f"Error: Video file not found at {video_path}")
        sys.exit(1)

    # 3. Configure the PoseLandmarker for VIDEO mode
    base_options = python.BaseOptions(model_asset_path='./pose_landmarker_heavy.task')
    options = vision.PoseLandmarkerOptions(
        base_options=base_options,
        running_mode=vision.RunningMode.VIDEO, # Tell it we are feeding a video stream
        output_segmentation_masks=False # Turned off to save processing power on the server
    )
    
    detector = vision.PoseLandmarker.create_from_options(options)

    # 4. Open the video file
    cap = cv2.VideoCapture(video_path)
    fps = cap.get(cv2.CAP_PROP_FPS)
    if fps == 0:
        fps = 30 # Fallback if FPS cannot be read
        
    frame_index = 0
    total_frames_analyzed = 0
    frames_with_poor_form = 0

    # 5. Loop through the video frame-by-frame
    while cap.isOpened():
        ret, frame = cap.read()
        if not ret:
            break # Reached the end of the video

        # Convert OpenCV's BGR format to MediaPipe's RGB format
        rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=rgb_frame)
        
        # Calculate the timestamp (required for VIDEO mode)
        timestamp_ms = int((frame_index / fps) * 1000)

        # Detect pose
        detection_result = detector.detect_for_video(mp_image, timestamp_ms)

        # 6. Perform your "Form Check" logic
        if detection_result.pose_landmarks:
            landmarks = detection_result.pose_landmarks[0]
            total_frames_analyzed += 1
            
            # --- EXAMPLE LOGIC: Check if head is dropping (Slouching) ---
            # Landmark 7 is Left Ear, Landmark 11 is Left Shoulder
            # In image coordinates, lower Y value means higher up physically.
            left_ear_y = landmarks[7].y
            left_shoulder_y = landmarks[11].y
            
            if left_ear_y > left_shoulder_y:
                frames_with_poor_form += 1

        frame_index += 1

    cap.release()
    detector.close()

    # 7. Print the final output so C# can capture it!
    if total_frames_analyzed == 0:
        print("Analysis Failed: No person detected in the video.")
        sys.exit(1)

    mistake_percentage = (frames_with_poor_form / total_frames_analyzed) * 100

    # This exact string is what gets saved to your WorkoutSession.AiFeedback in the Database!
    if mistake_percentage > 20:
        print(f"Poor Form: You dropped your head in {mistake_percentage:.1f}% of the movement. Keep your chest up.")
    else:
        print(f"Great Job: Form was solid. Deviations detected in only {mistake_percentage:.1f}% of frames.")

if __name__ == "__main__":
    main()