# OPTI-SEC: Optimized Biometric Access Control Platform

[![Framework](https://img.shields.io/badge/Backend-.NET%209.0%20Core-purple)](#)
[![Mobile](https://img.shields.io/badge/Mobile-Flutter%203.x-cyan)](#)
[![AI](https://img.shields.io/badge/AI-PyTorch%20%7C%20FastAPI-green)](#)
[![Container](https://img.shields.io/badge/DevOps-Docker%20Compose-blue)](#)

OPTI-SEC هو نظام أمني متكامل لإدارة الصلاحيات والتحكم في البوابات الذكية يعتمد على توثيق الهوية عبر ثلاثة عوامل متتالية (3-Factor Authentication) للقضاء تماماً على ثغرات الأنظمة التقليدية والتصدي لهجمات التزييف الحيوية (Presentation Attacks).

## 🧠 بنية النظام والتحقق الثلاثي (Authentication Pipeline)

يعتمد النظام على منطق تسلسلي صارم وموزع بين الأنظمة المدمجة (Embedded) والسيرفر السحابي (Cloud Backend) لضمان عدم وجود نقطة فشل واحدة (Single Point of Failure):

1. **Phase 1 (Password Phase)**: إدخال رمز الـ PIN عبر لوحة المفاتيح 4x4 وتشفيره والتحقق منه عبر الـ .NET Backend، ويتضمن النظام بروتوكول الإنذار الصامت للسرقة أو الإكراه (Silent Duress Protocol) عبر الكود الخفي لإرسال تنبيه فوري لهواتف الإدارة دون إشعار المعتدي[cite: 1].
2. **Phase 2 (Camera & AI Phase)**: عند نجاح الرمز، تلتقط كاميرا ESP32-CAM الصورة وترسلها مباشرة إلى حاوية الـ Python AI Microservice للتحقق من حيوية الوجه (Liveness Detection Engine) لمنع الاختراق بالصور أو الشاشات، واستخراج الأبعاد الوجيهة[cite: 1].
3. **Phase 3 (Fingerprint & Matching Phase)**: يتم قراءة بصمة الإصبع عبر مستشعر R307 ومطابقتها تقاطعياً مع الـ User_ID المستخرج من معالج الذكاء الاصطناعي لتأكيد الصلاحية النهائية[cite: 1].

### 📐 المخطط الرياضي لمطابقة ميزات الوجه (Euclidean Vector)
يقوم نموذج الـ ResNet-34 بتحويل الصورة إلى متجه مكون من 128 بعداً عائماً ($V_{live}$) ومقارنته بالبصمة المخزنة ($V_{stored}$) عبر معادلة المسافة الإقليدية[cite: 1]:

$$d(V_{live}, V_{stored}) = \sqrt{\sum_{i=1}^{128} (V_{live, i} - V_{stored, i})^2}$$

ويتم قبول الهوية فقط إذا كانت قيمة المسافة أصغر من العتبة الأمنية المعايرة:
$$\min(d) < 0.50$$

---

## 🛠️ تفاصيل التوصيل والعتاد (Hardware Interfacing)

| المكون الإلكتروني | منفذ الـ GPIO | وظيفة المكون في المنظومة الأمنيّة |
| :--- | :--- | :--- |
| **ESP32-S3 Core** | المعالج الرئيسي | إدارة الـ State Machine والاتصال بالـ REST APIs عبر الـ HTTPS[cite: 1]. |
| **R307 Optical Sensor**| GPIO16 (RX2), GPIO17 (TX2) | سحب خرائط البصمات الحيوية وإدارتها بتردد 57600 baud[cite: 1]. |
| **4x4 Matrix Keypad** | GPIO12, 13, 14, 27, 26, 33, 32 | تجميع الرموز الرقمية مع ميزة Software Debouncing[cite: 1]. |
| **ESP32-CAM Module** | GPIO4 (Trigger), Shared I2C | التقاط وتمرير صور VGA (640x480) بصيغة JPEG مضغوطة[cite: 1]. |
| **I2C 16x2 LCD** | GPIO22 (SDA), GPIO23 (SCL) | طباعة التوجيهات الحية للمستخدم طبقاً للحالة البرمجية[cite: 1]. |
| **Laser & LDR Circuit**| GPIO34 (Analog Input) | حاجز حماية ومراقبة محيطي متكامل (إذا كانت القراءة < 500)[cite: 1]. |
| **2-Channel Relay** | GPIO18 (Gate Lock), GPIO25 | التحكم في قفل البوابة الكهربائي والمسرّع الصوتي للإنذار[cite: 1]. |

---

## 🐳 بيئة التشغيل والحاويات (Docker Compose Architecture)

تم بناء وإدارة السيرفر الخلفي وقواعد البيانات عبر بيئة حاويات معزولة بالكامل لضمان الكفاءة والأمان[cite: 1]:

```yaml
version: '3.8'

services:
  smart-gate-db:
    image: [mcr.microsoft.com/mssql/server:2022](https://mcr.microsoft.com/mssql/server:2022)
    environment:
      - SA_PASSWORD=YourSecure_SA_Pass123!
      - ACCEPT_EULA=Y
    volumes:
      - mssql_data:/var/opt/mssql

  smart-gate-backend:
    image: optisec-backend:latest
    ports:
      - "5000:80"
    depends_on:
      smart-gate-db:
        condition: service_healthy
    volumes:
      - shared_images:/app/wwwroot/Images

  smart-gate-ai:
    image: optisec-ai:latest
    ports:
      - "8000:8000"
    volumes:
      - shared_images:/app/images