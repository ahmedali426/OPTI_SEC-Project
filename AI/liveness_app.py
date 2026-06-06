import cv2
import numpy as np
import pickle
import requests
import face_recognition
import time
import uuid
import firebase_admin
from firebase_admin import credentials, db
from fastapi import FastAPI, UploadFile, File, Form
from typing import Optional

app = FastAPI(title="Smart Gate Security API")

MODEL_PATH = "trained_faces.pkl"
DOTNET_BACKEND_URL = "https://opti-sec.runasp.net/api/AICallback/recognition-result"

CURRENT_SESSION_TOKEN = "No-Active-Session"

def init_firebase():
    try:
        cred = credentials.Certificate("ServiceAccountKey.json")  
        firebase_admin.initialize_app(cred, {
            'databaseURL': 'https://opti-sec-default-rtdb.europe-west1.firebasedatabase.app/' 
        })
        print(" Firebase SDK Initialized Successfully!")
        listen_to_firebase()
    except Exception as e:
        print(f" Failed to initialize Firebase: {e}")

def firebase_update(event):
    global CURRENT_SESSION_TOKEN
    if event.path == "/sessionToken" and event.data:
        CURRENT_SESSION_TOKEN = str(event.data)
        print(f" [Firebase Update] New SessionToken Detected: {CURRENT_SESSION_TOKEN}")
    elif event.path == "/" and event.data and "sessionToken" in event.data:
        CURRENT_SESSION_TOKEN = str(event.data["sessionToken"])
        print(f" [Firebase Update] Active SessionToken Updated: {CURRENT_SESSION_TOKEN}")

def listen_to_firebase():
    try:
        ref = db.reference('/')  
        ref.listen(firebase_update)
        print(" Listening to Firebase Realtime Database changes...")
    except Exception as e:
        print(f" Firebase Listener Error: {e}")

init_firebase()

def load_trained_faces():
    try:
        with open(MODEL_PATH, "rb") as f:
            return pickle.load(f)
    except FileNotFoundError:
        return {"encodings": [], "names": [], "ids": []}
    except Exception:
        return {"encodings": [], "names": [], "ids": []}

def get_liveness_score(face_crop):
    gray = cv2.cvtColor(face_crop, cv2.COLOR_BGR2GRAY)
    std_dev = np.std(gray)
    score = (std_dev / 40.0)
    return min(max(score, 0.0), 1.0)

@app.post("/scan-gate")
async def scan_gate(file: UploadFile = File(...)):
    global CURRENT_SESSION_TOKEN
    start_time = time.time()
    contents = await file.read()
    nparr = np.frombuffer(contents, np.uint8)
    frame = cv2.imdecode(nparr, cv2.IMREAD_COLOR)
    
    if frame is None:
        return {"status": "Error", "message": "Invalid image received"}
    token_to_send = CURRENT_SESSION_TOKEN

    rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    face_locations = face_recognition.face_locations(rgb_frame)
    
    # 1. حالة عدم وجود وجه
    if not face_locations:
        processing_time = int((time.time() - start_time) * 1000)
        
        data_payload = {
            "SessionToken": token_to_send,
            "IsAuthorized": "false",
            "ConfidenceScore": "0",
            "ProcessingTimeMs": str(processing_time)
        }
        files = {"ImageUrl": ("no_face.jpg", contents, "image/jpeg")}
        
        try:
            requests.post(DOTNET_BACKEND_URL, data=data_payload, files=files, timeout=4)
        except Exception as e:
            print(f"Dotnet status: Backend unreachable ({e})")
            
        return {"status": "Unauthorized", "message": "No face detected"}
    
    top, right, bottom, left = face_locations[0]
    face_crop = frame[top:bottom, left:right]
    
    liveness_score = get_liveness_score(face_crop)
    calculated_confidence = str(round(liveness_score * 100, 2))

    # 2. حالة فشل الحيوية (هجوم اختراق)
    if liveness_score < 0.3:
        processing_time = int((time.time() - start_time) * 1000)
        
        data_payload = {
            "SessionToken": token_to_send,
            "IsAuthorized": "false",
            "ConfidenceScore": calculated_confidence,
            "ProcessingTimeMs": str(processing_time)
        }
        files = {"ImageUrl": ("spoof_attack.jpg", contents, "image/jpeg")}
        
        try:
            requests.post(DOTNET_BACKEND_URL, data=data_payload, files=files, timeout=4)
        except Exception as e:
            print(f"Dotnet status: Backend unreachable ({e})")
            
        return {"status": "Rejected", "reason": "Liveness check failed (Spoof attack)", "liveness_score": round(liveness_score, 2)}

    face_encodings = face_recognition.face_encodings(rgb_frame, face_locations)
    if len(face_encodings) == 0:
        return {"status": "Unauthorized", "message": "Could not extract face encodings"}
        
    current_encoding = face_encodings[0]
    
    data = load_trained_faces()
    is_authorized = False
    recognized_username = "Unknown"
    matched_id = ""
    
    if data and "encodings" in data and len(data["encodings"]) > 0:
        known_encodings = [np.array(enc, dtype=np.float64) for enc in data["encodings"]]
        matches = face_recognition.compare_faces(known_encodings, current_encoding, tolerance=0.5)
        
        if True in matches:
            first_match_index = matches.index(True)
            recognized_username = data["names"][first_match_index]
            if "ids" in data and len(data["ids"]) > first_match_index:
                matched_id = str(data["ids"][first_match_index])
            else:
                matched_id = "1"
            is_authorized = True

    processing_time = int((time.time() - start_time) * 1000)

    # 3. حالة تم التعرف على الوش والحيوية تمام (Authorized)
    if is_authorized:
        #  تحديث الـ nextStep في فايربيز فوراً لتوجيه الـ ESP32 للخطوة التالية
        try:
            db.reference('/').update({"nextStep": "CaptureFingerprint"})
            print("📡 [Firebase Write] nextStep updated to 'CaptureFingerprint' successfully!")
        except Exception as fe:
            print(f" Failed to update nextStep in Firebase: {fe}")

        data_payload = {
            "SessionToken": token_to_send,
            "IsAuthorized": "true",
            "ConfidenceScore": calculated_confidence,
            "MatchedMemberId": matched_id,
            "ProcessingTimeMs": str(processing_time)
        }
        files = {"ImageUrl": ("authorized.jpg", contents, "image/jpeg")}
        
        try:
            requests.post(DOTNET_BACKEND_URL, data=data_payload, files=files, timeout=4)
        except Exception as e:
            print(f"Dotnet status: Backend unreachable ({e})")
            
        return {"status": "Authorized", "UserName": recognized_username, "liveness_score": round(liveness_score, 2), "session_token": token_to_send}
    
    # 4. حالة وش حقيقي بس مش متسجل في السيستم (Unauthorized Face)
    else:
        data_payload = {
            "SessionToken": token_to_send,
            "IsAuthorized": "false",
            "ConfidenceScore": calculated_confidence,
            "ProcessingTimeMs": str(processing_time)
        }
        files = {"ImageUrl": ("unauthorized.jpg", contents, "image/jpeg")}
        
        try:
            requests.post(DOTNET_BACKEND_URL, data=data_payload, files=files, timeout=4)
        except Exception as e:
            print(f"Dotnet status: Backend unreachable ({e})")
            
        return {"status": "Unauthorized", "message": "Face not recognized", "session_token": token_to_send}

@app.post("/add-user-train")
async def add_user_train(
    id: str = Form(...), 
    username: str = Form(...), 
    file: UploadFile = File(...)
):
    contents = await file.read()
    nparr = np.frombuffer(contents, np.uint8)
    image = cv2.imdecode(nparr, cv2.IMREAD_COLOR)
    
    if image is None:
        return {"status": "Failed", "message": "Invalid image format"}

    rgb_image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
    encodings = face_recognition.face_encodings(rgb_image)
    
    if len(encodings) == 0:
        return {"status": "Failed", "message": "No face found in the image to train"}
        
    data = load_trained_faces()
    if "encodings" not in data or not isinstance(data, dict):
        data = {"encodings": [], "names": [], "ids": []}
    if "ids" not in data:
        data["ids"] = []
        
    data["encodings"].append(encodings[0].tolist())
    data["names"].append(username)
    data["ids"].append(id)
    
    with open(MODEL_PATH, "wb") as f:
        pickle.dump(data, f)
        
    return {"status": "Success", "message": f"User {username} with ID {id} trained and saved successfully!"}