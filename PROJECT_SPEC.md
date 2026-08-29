\# 📌 PROJECT\_SPEC.md: EasyTrack (Expense \& Income Tracker)



เอกสารข้อกำหนดทางเทคนิคและการทำงานของระบบ (Master Technical \& Feature Specification) สำหรับการพัฒนาเว็บแอปพลิเคชันบันทึกรายรับ-รายจ่าย ด้วย AI-Assisted Engineering



\---



\## 🛠 Tech Stack Overview



\- \*\*Frontend:\*\* Nuxt 4.5.1, TypeScript, Tailwind CSS, Pinia, Lucide Icons

\- \*\*Backend Architecture:\*\* Microservices Architecture (ASP.NET Core Web API / MVC Pattern)

\- \*\*Backend Tech:\*\* .NET 8 / 9 (C#), Entity Framework Core (EF Core)

\- \*\*Database:\*\* PostgreSQL (with `uuid-ossp`)

\- \*\*Storage:\*\* S3-compatible Object Storage (AWS S3 / MinIO / Cloudflare R2) สำหรับจัดเก็บรูปสลิป

\- \*\*Containerization:\*\* Docker, Docker Compose (Multi-container setup)

\- \*\*Authentication:\*\* JWT Bearer Authentication (Access Token \& Refresh Token)



\---



\## 📁 Repository \& Document Structure



เมื่อเริ่มต้นโปรเจกต์ ให้สร้างโฟลเดอร์ `docs/` เพื่อแยกเก็บ Spec ของแต่ละฟีเจอร์สำหรับการพัฒนาและต่อยอดในอนาคต:



```text

easytrack/

├── docs/

│   ├── architecture/

│   │   └── system-overview.md

│   └── features/

│       ├── 01-auth-and-user.md          # รายละเอียดระบบ Login / Register / JWT

│       ├── 02-transaction-management.md # รายละเอียด CRUD รายรับ-รายจ่าย \& หมวดหมู่

│       ├── 03-slip-upload.md            # รายละเอียดการอัปโหลดและจัดการรูปสลิป

│       ├── 04-analytics-reporting.md    # รายละเอียดการคำนวณสรุป สัปดาห์/เดือน/ปี

│       └── 05-mobile-responsive-ui.md   # รายละเอียด UI/UX Component Specifications

├── backend/

│   ├── src/

│   │   ├── Services/

│   │   │   ├── AuthService/

│   │   │   ├── TransactionService/

│   │   │   └── MediaService/

│   │   └── Shared/

│   └── Dockerfile.\*

├── frontend/

│   ├── app/

│   ├── nuxt.config.ts

│   └── Dockerfile

├── docker-compose.yml

└── PROJECT\_SPEC.md

