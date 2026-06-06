
import cv2
import face_recognition
import pickle
import numpy as np
from tkinter import filedialog, Tk
from liveness_app import get_liveness_score

def is_live_face(face_crop):
    
    gray = cv2.cvtColor(face_crop, cv2.COLOR_BGR2GRAY)
    
  
    std_dev = np.std(gray)
    
   
    laplacian_var = cv2.Laplacian(gray, cv2.CV_64F).var()
    
    print(f"DEBUG: StdDev={std_dev:.2f}, Laplacian={laplacian_var:.2f}")
    
    
    return std_dev > 10 and laplacian_var > 40

def check_authorization(image_path):
    frame = cv2.imread(image_path)
    if frame is None: return "Error: Could not load image"

    rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    face_locations = face_recognition.face_locations(rgb_frame)
    
    if not face_locations:
        return "Unauthorized: No face detected"
    
    
    top, right, bottom, left = face_locations[0]
    face_crop = frame[top:bottom, left:right]
   
    if not is_live_face(face_crop):
        return "Spoof Detected"

    with open('trained_faces.pkl', 'rb') as f:
        data = pickle.load(f)
    
    encodings = face_recognition.face_encodings(rgb_frame, face_locations)
    
    matches = face_recognition.compare_faces(data['encodings'], encodings[0], tolerance=0.4)
    
    if True in matches:
        return "Authorized"
    else:
        return "Unauthorized"

if __name__ == "__main__":
    
    root = Tk()
    root.withdraw()
    path = filedialog.askopenfilename(title="Select Image to Verify")
    if path:
        result = check_authorization(path)
        print(f"Final Result: {result}")