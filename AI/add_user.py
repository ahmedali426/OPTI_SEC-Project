import os
import shutil
import tkinter as tk
from tkinter import filedialog, messagebox
import face_recognition
import pickle

# دالة تحديث الموديل (تضيف بيانات الشخص الجديد فقط)
def train_system(new_person_name, source_path):
    print(f"--- Processing {new_person_name}: Extracting Encodings ---")
    
    # تحميل البيانات الموجودة أو إنشاء قائمة جديدة
    if os.path.exists('trained_faces.pkl'):
        with open('trained_faces.pkl', 'rb') as f:
            data = pickle.load(f)
    else:
        data = {"encodings": [], "names": []}

    # معالجة الصور داخل المجلد المختار
    for filename in os.listdir(source_path):
        if filename.lower().endswith(('.jpg', '.jpeg', '.png')):
            image_path = os.path.join(source_path, filename)
            image = face_recognition.load_image_file(image_path)
            
            # استخراج البصمة
            encodings = face_recognition.face_encodings(image)
            
            if len(encodings) > 0:
                # إضافة البصمة والاسم
                data["encodings"].append(encodings[0])
                data["names"].append(new_person_name)
    
    # حفظ البيانات المحدثة
    with open('trained_faces.pkl', 'wb') as f:
        pickle.dump(data, f)
    
    print(f"Successfully updated system with {new_person_name}.")

def add_user_process():
   
    person_name = input("Enter the name of the new person: ").strip()
    if not person_name:
        print("Error: Name is required.")
        return

   
    root = tk.Tk()
    root.withdraw()
    source_folder = filedialog.askdirectory(title=f"Select Folder containing photos for {person_name}")

    if not source_folder:
        print("No folder selected. Operation cancelled.")
        return

   
    base_dir = os.path.dirname(os.path.abspath(__file__))
    database_dir = os.path.join(base_dir, 'database')
    user_folder = os.path.join(database_dir, person_name)
    
    if not os.path.exists(user_folder):
        os.makedirs(user_folder)

    
    for filename in os.listdir(source_folder):
        if filename.lower().endswith(('.jpg', '.jpeg', '.png')):
            shutil.copy(os.path.join(source_folder, filename), user_folder)

    train_system(person_name, user_folder)
    
    print(f"\nSUCCESS: {person_name} has been added successfully.")

if __name__ == "__main__":
    add_user_process()