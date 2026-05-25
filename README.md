# نظام إحصاء السكن الريفي 2026
## HabitatRural – Gestion des Annexes 05 & 06

[![Build](https://github.com/YOUR_USERNAME/HabitatRural/actions/workflows/build.yml/badge.svg)](https://github.com/YOUR_USERNAME/HabitatRural/actions/workflows/build.yml)

---

### 📋 وصف التطبيق

تطبيق C# / WPF بواجهة عربية كاملة لتحرير وطباعة:
- **الملحق 05** – طلب صرف مساعدة السكن الريفي
- **الملحق 06** – محضر معاينة أشغال السكن الريفي

يحترم التطبيق تماماً الشكل الرسمي للنماذج (قياسات A4 بالملمتر).

---

### ✨ الميزات

| الميزة | التفاصيل |
|--------|----------|
| 🗃 قاعدة بيانات | SQLite محلية – لا حاجة لتثبيت خادم |
| 🖨 طباعة دقيقة | النماذج تُرسم برمجياً بقياسات ملمترية دقيقة |
| 👁 معاينة | DocumentViewer مدمج قبل الطباعة |
| 🔍 بحث | بحث فوري في قائمة المستفيدين |
| 🌐 عربي RTL | الواجهة كاملاً باللغة العربية من اليمين لليسار |

---

### 🚀 متطلبات التشغيل

- **Windows 10/11** (64-bit)
- **.NET 8** Runtime – [تحميل](https://dotnet.microsoft.com/download/dotnet/8.0)
- أو تحميل النسخة المحمولة `HabitatRural.exe` من Releases (self-contained)

---

### 🔨 بناء المشروع محلياً

```bash
# استنساخ المشروع
git clone https://github.com/YOUR_USERNAME/HabitatRural.git
cd HabitatRural

# بناء
dotnet build HabitatRural/HabitatRural.csproj -c Release

# تشغيل
dotnet run --project HabitatRural/HabitatRural.csproj

# نشر كملف exe واحد
dotnet publish HabitatRural/HabitatRural.csproj \
  -c Release -r win-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -o ./publish
```

---

### 📁 هيكل المشروع

```
HabitatRural/
├── .github/workflows/build.yml    # GitHub Actions
├── HabitatRural.sln
└── HabitatRural/
    ├── Models/
    │   ├── Beneficiaire.cs         # بيانات المستفيد
    │   ├── DemandeVersement.cs     # الملحق 05
    │   └── ProcesVerbal.cs         # الملحق 06
    ├── Data/
    │   └── DatabaseService.cs      # كل عمليات SQLite
    ├── Views/
    │   ├── BeneficiaireEditDialog  # تحرير بيانات المستفيد
    │   ├── Annexe05Dialog          # تحرير الملحق 05
    │   ├── Annexe06Dialog          # تحرير الملحق 06
    │   └── PrintPreviewDialog      # معاينة قبل الطباعة
    ├── Printing/
    │   ├── FormHelper.cs           # أدوات الرسم (mm → px)
    │   ├── Annexe05Renderer.cs     # رسم النموذج 05
    │   └── Annexe06Renderer.cs     # رسم النموذج 06
    ├── MainWindow.xaml             # الواجهة الرئيسية
    └── App.xaml                    # إعدادات التطبيق
```

---

### 📦 إصدار GitHub Actions

عند كل `push` على `main` يقوم GitHub Actions تلقائياً بـ:
1. بناء المشروع (`dotnet build`)
2. نشر ملف `HabitatRural.exe` واحد مكتفٍ بذاته
3. رفعه كـ **Artifact** جاهز للتحميل

---

### 🗺 خريطة الطريق

- [ ] تصدير التقرير كـ PDF
- [ ] بحث متقدم (بالتاريخ، البلدية، حالة الملف)
- [ ] إحصائيات عامة (عدد الملفات حسب الشريحة)
- [ ] نسخ احتياطي تلقائي لقاعدة البيانات
