# استخدام النسخة الجاهزة والمبنية للـ face_recognition
FROM animcogn/face_recognition:latest

WORKDIR /app

# نسخ ملف الـ requirements وتثبيت باقي المكتبات مباشرة
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# نسخ باقي ملفات الكود
COPY . .

EXPOSE 8000

CMD ["uvicorn", "liveness_app:app", "--host", "0.0.0.0", "--port", "7860"]