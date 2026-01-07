# LabTrack Lite

LabTrack Lite is a full‑stack web application for managing laboratory assets and maintenance tickets. It provides workflow enforcement, role‑based access control (RBAC), audit logging, secure REST APIs, PostgreSQL persistence, and a natural‑language chatbot interface for quick queries.

---

## Table of Contents
- [Features](#features)
- [System Architecture](#system-architecture)
- [User Roles](#user-roles)
- [API Endpoints](#api-endpoints)
- [Chatbot Queries](#chatbot-queries)
- [Running Locally](#running-locally)
  - [Backend](#backend)
  - [Frontend](#frontend)
  - [Database](#database)
- [Demo Credentials](#demo-credentials)
- [Demo Flow](#demo-flow)
- [Development Notes](#development-notes)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)

---

## Features
- Asset registration and tracking
- Maintenance ticket creation and lifecycle / workflow enforcement
- Role‑Based Access Control (Admin, Engineer, Technician)
- Natural‑language query chatbot (rule‑based NLP)
- Audit logs for all operations (who/what/when)
- Secure REST APIs (CORS, role checks)
- PostgreSQL data persistence

---

## System Architecture

| Layer     | Technology                       |
|-----------|----------------------------------|
| Frontend  | React (Vite)                     |
| Backend   | ASP.NET Core Minimal APIs        |
| Database  | PostgreSQL                       |
| Chatbot   | Rule‑based NLP module (backend)  |
| Security  | CORS, RBAC, Audit logs, JWT*     |

\* If authentication is implemented with JWT or another token system — adjust as implemented.

---

## User Roles

| Role       | Responsibilities |
|------------|------------------|
| Admin      | Manage users, assets, tickets, and configuration |
| Engineer   | Resolve and close tickets, update maintenance records |
| Technician | Report issues, perform updates, change ticket status |

---

## API Endpoints

> Base URL: http://localhost:5000 (example — use your configured port/ref)

| Method | Endpoint           | Description                |
|--------|--------------------|----------------------------|
| POST   | /login             | User login (returns token/session) |
| GET    | /assets            | List/view assets           |
| POST   | /assets            | Add a new asset            |
| GET    | /tickets           | List/view tickets          |
| POST   | /tickets           | Create a ticket            |
| POST   | /chat/query        | Chatbot query (NLQ)        |

Example: POST /login
Request (JSON):
```json
{
  "username": "admin",
  "password": "admin"
}
```

Example: POST /assets
Request (JSON):
```json
{
  "tag": "MICRO-001",
  "name": "Microscope Model X",
  "location": "Lab A",
  "category": "Optical",
  "purchaseDate": "2024-08-12",
  "metadata": {
    "serial": "SN-12345"
  }
}
```

Example: POST /tickets
Request (JSON):
```json
{
  "assetTag": "MICRO-001",
  "reportedBy": "tech",
  "priority": "Medium",
  "title": "Objective lens damage",
  "description": "Lens has a scratch affecting imaging"
}
```

Example: POST /chat/query
Request (JSON):
```json
{
  "query": "open tickets for Lab A"
}
```

---

## Chatbot Queries (examples)
The chatbot is rule‑based and understands common queries such as:
- "open tickets"
- "count assets"
- "repair assets"
- "open tickets in Lab A"
- "ticket status for MICRO-001"

Payload: POST /chat/query with JSON { "query": "<your natural language query>" }  
Response: structured JSON containing the answer and any suggested actions.

---

## Running Locally

Prerequisites:
- .NET SDK (compatible with the backend)
- Node.js and npm
- PostgreSQL
- Optional: dotnet-ef for migrations

### Backend
1. Change to backend folder:
   cd backend/LabTrackLite
2. Configure environment variables (example .env or appsettings):
   - ConnectionStrings: PostgreSQL connection string (Host, Port, Database, Username, Password)
   - JWT or session secret (if used)
   - CORS origins
3. Run:
   dotnet run
4. API should be available at the configured host/port (e.g. http://localhost:5000)

Example environment variables (.env.example):
```env
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__Default=Host=localhost;Port=5432;Database=labtrack;Username=labuser;Password=labpass
JWT__Key=your_jwt_secret_here
CORS__AllowedOrigins=http://localhost:3000
```

### Database
1. Create DB and user in PostgreSQL:
   - createdb labtrack
   - createuser labuser (with password and privileges)
2. Run EF migrations (if applicable):
   dotnet ef database update
   (Or run any provided SQL migration scripts in /backend/Migrations)

### Frontend
1. Change to frontend:
   cd frontend
2. Install:
   npm install
3. Run dev server:
   npm run dev
4. Open browser at http://localhost:3000 (or the Vite-provided port)

---

## Demo Credentials

| Role       | Username | Password |
|------------|----------|----------|
| Admin      | admin    | admin    |
| Engineer   | eng      | eng      |
| Technician | tech     | tech     |

(These are demo/test credentials only — do not use in production.)

---

## Demo Flow
1. Start backend and frontend (see Running Locally).
2. Login as Admin.
3. Add an asset via UI or POST /assets.
4. Create a maintenance ticket for that asset (POST /tickets).
5. Login as Technician or Engineer to update ticket status.
6. Use Chatbot (POST /chat/query) for quick queries like "open tickets" or "count assets".

---

## Development Notes
- Audit logs capture who performed actions and when — verify backend configuration to ensure logs are persisted.
- RBAC checks should be implemented on protected endpoints; ensure middleware checks user roles before sensitive operations.
- The chatbot is a rule‑based module. For more advanced NLU, consider integrating an LLM or an intent‑recognition library and persist query logs for analytics.

---

## Contributing
Contributions are welcome. Suggested flow:
1. Fork the repository
2. Create a feature branch: git checkout -b feat/your-feature
3. Make changes and add tests where appropriate
4. Open a pull request describing your changes

Please keep changes small and focused. Follow existing code and commit style.

---

## License
This project is released under the MIT License. See LICENSE file for details.

---

## Contact
Maintainer: vigneshssv315  
GitHub: https://github.com/vigneshssv315/LabTrack-Lite

If you need help running the project or want to contribute a feature, open an issue or a PR on the repository.

```
