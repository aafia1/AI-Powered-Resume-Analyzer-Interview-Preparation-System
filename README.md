# 🧠 Resume Analyzer & Interview Preparation System

> ASP.NET Core 8 MVC web app that analyzes resumes, scores them for ATS-readiness, matches them against job descriptions, and generates interview questions.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![EF Core](https://img.shields.io/badge/EF%20Core-8.0-512BD4)
![SQL Server](https://img.shields.io/badge/Database-SQL%20Server-CC2927?logo=microsoftsqlserver)
![License](https://img.shields.io/badge/License-MIT-green)

---

## Overview

Resume Analyzer is a full-stack ASP.NET Core MVC application that lets users upload a resume (PDF/DOCX), automatically extracts and analyzes its content, and returns:

- A **Resume Score** (0–100) based on structure, skills, and completeness
- An **ATS Score** (0–100) estimating how well it would pass an Applicant Tracking System
- **Job Description matching** — paste a JD and get matched/missing skills with a compatibility %
- **Interview questions** generated from the detected skill set (Beginner / Intermediate / Advanced)
- A **downloadable PDF report** of the full analysis
- A **dashboard** with score trends and analytics (Chart.js)

## Features

| Module | Description |
|---|---|
| 🔐 Authentication | Register, Login, Logout, Forgot/Change Password, role-based access (Admin/User) via ASP.NET Identity |
| 📄 Resume Upload | Drag-and-drop PDF/DOCX upload with validation and text extraction |
| 🧠 AI-style Analysis | Skill detection, Resume Score, ATS Score, parsed contact info |
| 💼 Job Matching | Paste any job description → matched/missing skills, compatibility % |
| 🎤 Interview Prep | Skill-based question bank across 3 difficulty levels |
| 📊 Dashboard | Score trends and skill analytics with Chart.js |
| 🛡️ Admin Panel | User management, system stats, skills database |
| 📁 History | View, compare, and delete past resume analyses |
| 📑 PDF Reports | Downloadable analysis reports generated with iText7 |
| 🌙 Dark Mode | Full light/dark theme toggle |

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core 8 MVC (C#) |
| Database | SQL Server + Entity Framework Core 8 |
| Auth | ASP.NET Core Identity (roles: Admin, User) |
| Frontend | Bootstrap 5.3, Font Awesome 6, Chart.js 4 |
| PDF text extraction | PdfPig |
| DOCX text extraction | DocumentFormat.OpenXml |
| PDF report generation | iText7 |
| Serialization | Newtonsoft.Json |

## Project Structure

```
ResumeAnalyzer/
├── Controllers/          # HomeController, AccountController, ResumeController, AdminController, HelpController
├── Models/                # EF Core entities (Resume, Skill, JobDescription, AnalysisResult, Report, ...)
├── ViewModels/            # View-facing DTOs
├── Services/              # ResumeParserService, ResumeAnalysisService, PdfReportService
├── Data/                  # ApplicationDbContext (EF Core, seed data)
├── Views/                 # Razor views (Home, Account, Dashboard, Resume, Admin, Help, Shared)
├── Migrations/            # EF Core migrations
├── wwwroot/               # Static assets (css, js) + uploads/ (runtime files, gitignored)
├── Program.cs             # App bootstrap, middleware, DB migration + seeding
└── appsettings.json       # Configuration (connection string, app settings)
```

## Database Schema

8 tables via EF Core: `Users` (Identity), `Resumes`, `Skills`, `ResumeSkills` (join table), `JobDescriptions`, `AnalysisResults`, `InterviewQuestions`, `Reports`.

```
Users ─┬─ Resumes ─┬─ ResumeSkills ─── Skills
       │           ├─ AnalysisResults ─── JobDescriptions
       │           └─ Reports
       └─ InterviewQuestions (seeded, skill-linked)
```

## Scoring Logic

**Resume Score (0–100):** skills detected (up to 40 pts), contact info present (email/phone), experience/education/projects/certifications sections found, and word-count tier.

**ATS Score (0–100):** keyword density (30 pts), standard section headings (35 pts), clean-text ratio (15 pts), bullet-point usage (15 pts), keyword diversity (5 pts).

## Security Notes

- Passwords hashed via ASP.NET Core Identity
- Anti-forgery tokens enforced on all POST requests
- Role-based authorization (`[Authorize(Roles = "Admin")]`)
- HttpOnly, secure cookies
- File type/size validation on upload
- The app seeds a default admin (`admin@resumeanalyzer.com` / `Admin@123`) and demo user on first run for evaluation convenience — change or remove this before any real/public deployment.

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB, Express, or full) — LocalDB ships with Visual Studio
- Visual Studio 2022, VS Code, or Rider

### 1. Clone the repository

```bash
git clone https://github.com/<aafia1>/ResumeAnalyzer.git
cd ResumeAnalyzer
```

### 2. Configure the connection string

Edit `appsettings.json` if your SQL Server setup differs from the default LocalDB:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ResumeAnalyzerDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

For SQL Server Express:
```
Server=.\SQLEXPRESS;Database=ResumeAnalyzerDb;Trusted_Connection=True;
```

### 3. Restore packages and apply migrations

```bash
dotnet restore
dotnet ef database update
```
> Migrations and seed data are also applied automatically on first run.

### 4. Run

```bash
dotnet run
```

Open `https://localhost:5001` (or `http://localhost:5000`).

### 5. Default seeded accounts

| Role | Email | Password |
|---|---|---|
| Admin | admin@resumeanalyzer.com | Admin@123 |
| Demo User | demo@resumeanalyzer.com | Demo@123 |

---


## Screenshots of web:
<img width="1366" height="658" alt="image" src="https://github.com/user-attachments/assets/ceaa86b8-9509-4ad2-82f4-f910d534b2ca" />
<img width="1365" height="657" alt="image" src="https://github.com/user-attachments/assets/7212ba06-dd1b-43dc-b83d-8e0e1acb8ac3" />
<img width="887" height="627" alt="image" src="https://github.com/user-attachments/assets/d5586289-9e69-4d01-b2f4-257efd51daf0" />
<img width="1362" height="647" alt="image" src="https://github.com/user-attachments/assets/966096aa-5a00-40de-9c13-e6a92e0f6627" />

---

## Deployment

The app is a standard ASP.NET Core 8 MVC project and can be deployed anywhere .NET 8 + SQL Server are supported:

- **Azure App Service** + **Azure SQL Database** (most straightforward for .NET)
- **Render** or **Railway** using a Docker image
- **IIS** on a Windows server

## License

This project is available under the MIT License. See [LICENSE](LICENSE) for details.
