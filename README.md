# IT Help Desk & Ticketing Management System

A full-stack web application that helps companies manage internal IT support requests. Employees submit tickets, managers assign them to agents, and agents resolve them — all in one place.

---

## 1. Project Overview

Every company deals with the same problem: employees run into IT issues (a broken laptop, a locked account, a software bug) and someone needs a clear, trackable way to report it and get it fixed. This system is an internal tool that replaces messy email chains and verbal requests with a structured ticketing workflow.

Employees log in and describe their problem in a ticket. A manager reviews it and hands it to the right IT support agent, who works the issue and resolves it. Admins oversee the whole system — users, categories, holidays, and settings. Everyone involved gets notified the moment something changes on a ticket they care about, either inside the app or by email.

The system is built for internal company use only (not a public product), and it was developed to mirror how a real help desk operates day to day: tickets flow through clear stages, workloads are kept balanced across agents, and every action is recorded for accountability.

---

## 2. Features

### Ticket Management
- Create, edit, and track support tickets
- Categories: Hardware, Software, Network, Email, Access Request, Other
- Priority levels: Low, Medium, High, Critical
- Statuses: Open, In Progress, Pending, Resolved, Closed
- Unique reference number for every ticket (e.g. `TKT-006`)
- File attachments (upload screenshots, documents, logs)
- Comment system with chat-style replies
- Internal notes visible only to agents and admins (hidden from employees)
- Activity history timeline on every ticket

### User Roles & Access
- 4 roles with different permissions: Employee, Agent, Manager, Admin
- No self-registration — only admins can create accounts
- Each role sees only what they need

### Ticket Workflow
- Employee creates a ticket → Manager assigns it to an available agent → Agent resolves it
- Escalation: agents can escalate difficult tickets back to managers with a reason
- Agent workload cap: no agent is assigned more than 3 open tickets at once
- Escalated tickets go back to the manager for reassignment

### Notifications
- In-app notification bell with an unread badge
- Full notification center page per user (each person only sees their own notifications)
- Real-time push notifications using SignalR (a technology that pushes updates to the browser instantly, no page refresh needed)
- Email notifications for important events (ticket assigned, escalated, resolved)
- Emails include the ticket number, description, and a direct link to the ticket

### Dashboards
- Role-specific dashboards: Employee, Agent, Manager, and Admin each see different data
- Time-window filter: 7, 14, or 30 days
- Charts for ticket volume, categories, and priorities
- Agent performance tracking: resolved tickets and average resolution time

### Reports & Exports
- Monthly ticket volume report
- Average resolution time report
- Per-agent performance report
- Per-employee activity report
- Export any report to PDF (with charts and tables) or Excel

### AI Features (powered by Groq / Llama 3)
- AI ticket categorization: suggests the right category from the ticket description
- AI priority suggestion: recommends how urgent a ticket is

### Knowledge Base
- Admins create and approve help articles
- Employees can search and browse articles by category
- Reduces repetitive tickets by giving employees self-service answers

### Admin Panel
- User creation and management (create, edit, deactivate accounts)
- Role assignment
- Ticket category management
- Holiday calendar: add company holidays, viewable as a monthly calendar or a list
- System information dashboard (total users, tickets, storage used, server time)
- System settings (SMTP email configuration, file upload limits)

### Profile Management
- Every user can view and edit their own profile
- Upload a profile picture
- Change password securely
- View personal ticket stats

### Security
- JWT-based authentication (tokens expire and are never stored as plain text)
- Passwords hashed with BCrypt (a one-way hashing algorithm — passwords are never stored as plain text)
- File uploads validated by both file type and "magic bytes" (the actual binary signature of a file), blocking dangerous files like `.exe` or `.php` even if renamed
- Files are served only through authenticated endpoints — there is no direct public URL access
- All API routes are protected — unauthenticated requests are rejected

---

## 3. Tech Stack

### Backend
| Technology | What it does |
|---|---|
| ASP.NET Core Web API | The server that handles all the business logic and data |
| Clean Architecture | Code is organized into layers (Domain, Application, Infrastructure, Presentation) so it stays maintainable |
| Entity Framework Core | Lets the backend talk to the database without writing raw SQL |
| SQL Server | The database that stores all tickets, users, and system data |
| JWT Authentication | Secure login tokens that verify who a user is on every request |
| BCrypt | A secure way to hash and store passwords |
| SignalR | Enables real-time communication (live notifications without refreshing the page) |
| MailKit / System.Net.Mail | Sends email notifications via Gmail SMTP |
| QuestPDF | Generates PDF report exports |
| ClosedXML | Generates Excel report exports |
| Groq API (Llama 3) | Free AI API used for ticket categorization, priority suggestions, and the chat assistant |

### Frontend
| Technology | What it does |
|---|---|
| Vue 3 | The JavaScript framework that powers the user interface |
| Pinia | Manages shared state across the app (who is logged in, notifications, etc.) |
| Vue Router | Handles navigation between pages |
| Tailwind CSS v3 | Utility-based styling that keeps the UI consistent and responsive |
| Lucide Icons | The icon library used throughout the app |
| vue-chartjs / Chart.js | Renders charts on dashboards and report pages |
| @microsoft/signalr | Connects the frontend to the real-time notification hub |

### Tools
- Visual Studio 2022 — backend IDE
- VS Code — frontend editor
- SQL Server Management Studio (SSMS) — database management
- Postman — API testing
- Git + GitHub Desktop — version control
- GitLens — Git history in VS Code

---

## 4. Project Structure

```
/
├── backend/
│   ├── Domain/
│   │   └── Entities/          # Core data models (Ticket, User, Category, ...)
│   ├── Application/
│   │   ├── Common/            # Shared settings and helpers (e.g. GroqSettings)
│   │   ├── DTOs/               # Data transfer objects used by the API
│   │   ├── Interfaces/         # Service contracts
│   │   ├── MappingProfiles/    # AutoMapper profiles
│   │   └── Services/           # Business logic
│   ├── Infrastructure/
│   │   ├── Data/                # EF Core DbContext and database seeding
│   │   ├── Repositories/        # Data access implementations
│   │   └── Services/            # Email, file storage, AI (Groq) integrations
│   └── Presentation/
│       ├── Controllers/         # API endpoints (Auth, Ticket, Admin, AI, Reports, ...)
│       └── Hubs/                 # SignalR real-time notification hub
├── frontend/
│   ├── src/
│   │   ├── api/           # Axios setup and API call modules
│   │   ├── components/    # Reusable UI components
│   │   ├── composables/   # Reusable Vue composition functions
│   │   ├── config/        # App-level configuration (nav links, etc.)
│   │   ├── router/        # Vue Router configuration
│   │   ├── store/         # Pinia state management
│   │   └── views/         # Page-level components (dashboards, tickets, admin, ...)
├── schema and diagrams/    # Database schema SQL and ER diagrams
├── wireframes/             # UI wireframe images
├── workflow/                # Workflow diagrams (ticket lifecycle, escalation, etc.)
└── README.md
```

---

## 5. Database Schema

| Table | Description |
|---|---|
| User | Stores all user accounts and their roles |
| Role | Defines the four roles (Admin, Manager, Agent, Employee) |
| Department | Organizes users by department |
| Ticket | The main table storing all support tickets |
| Category | Ticket categories (Hardware, Software, etc.) |
| Priority | Priority levels (Low, Medium, High, Critical) |
| TicketStatus | The possible statuses a ticket can have |
| TicketComment | Stores all comments and internal notes on tickets |
| TicketAttachment | Tracks uploaded files linked to tickets |
| Notification | Stores in-app notifications per user |
| ActivityLog | Records every action taken on every ticket (the audit trail) |
| Holiday | Stores company holidays shown in the admin calendar |

A full schema script and ER diagrams are available in [`schema and diagrams/`](schema%20and%20diagrams/).

---

## 6. How the Ticket Workflow Works

1. An employee logs in and describes their IT problem in a new ticket. The AI suggests a category and priority — they can accept or change it.
2. The manager receives a notification and sees the new ticket in their dashboard. They review it and assign it to an available agent (the system won't let them assign to an agent who already has 3 open tickets).
3. The assigned agent gets notified and sees the ticket in their queue. They work on it, add comments, and can attach files.
4. If the issue is too complex, the agent escalates it back to the manager with a reason. The manager reassigns it.
5. Once resolved, the agent marks the ticket as Resolved — no manager approval is required. The employee gets notified that their ticket has been resolved.
6. The manager and admin can see the full history of everything that happened on the ticket, from creation to resolution.

---

## 7. Getting Started (Setup Instructions)

### Prerequisites
- .NET 10 SDK
- Node.js (v18 or higher) and npm
- SQL Server Express (or any SQL Server edition)
- SQL Server Management Studio (SSMS) — optional but recommended
- Visual Studio 2022 (for backend) or VS Code (for frontend)

### Backend Setup
1. Clone the repository
2. Open the `backend/` folder in Visual Studio 2022 (or open `backend/backend.sln`)
3. Create `appsettings.Development.json` in the `backend/` folder (copy from `appsettings.json` as a template) — **never commit this file**
4. Fill in the connection string pointing to your local SQL Server instance
5. Fill in the `EmailSettings` block with your Gmail address and app password
6. Add a `GroqSettings` block with your Groq API key, model name, and base URL (a free key is available at [console.groq.com](https://console.groq.com))
7. Run `Update-Database` in the Package Manager Console to create the database and seed sample data
8. Run the project (`F5` in Visual Studio) — the API starts at `https://localhost:7091`
9. Open `https://localhost:7091/swagger` to verify the API is running

### Frontend Setup
1. Open a terminal in the `frontend/` folder
2. Run `npm install` to install dependencies
3. Check `frontend/src/api/axios.js` — the `API_ORIGIN` constant should match your backend's URL (defaults to `https://localhost:7091`)
4. Run `npm run serve` to start the development server
5. Open the URL shown in the terminal (usually `http://localhost:8080`)

### Default Login Accounts (Seeded Data)
> These accounts are created automatically by the database seeder for local development and testing. **Change these passwords immediately in any real deployment.**

| Role | Email | Password |
|---|---|---|
| Admin | admin@helpdesk.com | Admin@123 |
| Manager | manager@helpdesk.com | Manager@123 |
| Agent | agent@helpdesk.com | Agent@123 |
| Employee | employee@helpdesk.com | Employee@123 |

---

## 8. API Documentation

Once the backend is running, a full interactive Swagger UI is available at `/swagger` (e.g. `https://localhost:7091/swagger`), where every endpoint can be viewed and tested directly from the browser. All endpoints (except login) require a JWT token — get one by calling `POST /api/auth/login` with valid credentials, then include the returned token as a `Bearer` token in the `Authorization` header of subsequent requests. Endpoints are grouped by controller/tag: Auth, Ticket, Admin, AI, Dashboard, Reports, Notification, Profile, Holiday, and Lookup.

---

## 9. Environment Variables Reference

| Key | Description | Where to get it |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string | Your local SSMS instance |
| `Jwt:Key` / `Jwt:Issuer` / `Jwt:Audience` | Secret and identifiers used to sign and validate login tokens | Generate your own secret key for production |
| `EmailSettings:SenderEmail` | Gmail address used for sending notifications | Your Gmail account |
| `EmailSettings:SenderAppPassword` | Gmail app password (not your real password) | Google Account → Security → App Passwords |
| `EmailSettings:SmtpHost` | SMTP server address | Fixed: `smtp.gmail.com` |
| `EmailSettings:SmtpPort` | SMTP port | Fixed: `587` |
| `GroqSettings:ApiKey` | API key for Groq AI | [console.groq.com](https://console.groq.com) (free) |
| `GroqSettings:Model` | AI model to use | e.g. a Llama 3 model available on Groq |
| `GroqSettings:BaseUrl` | Groq API base URL | Provided by Groq's documentation |
| `AppBaseUrl` | Base URL of the frontend (used to build links in emails) | e.g. `http://localhost:8080` |
| `FileUpload:MaxSizeMB` | Max allowed file upload size | Set to your preference, default `10` |

---

## 10. Security Notes

- No self-registration — accounts are admin-created only, preventing unauthorized access
- Passwords are never stored as plain text — BCrypt hashing with salt is used instead
- API keys and credentials must always go in `appsettings.Development.json`, never in `appsettings.json` (which is committed to GitHub)
- File uploads are validated by both file extension AND file content (magic bytes) — a renamed `.exe` disguised as a `.png` is still blocked
- Files are never served directly — all downloads go through an authenticated API endpoint
- JWT tokens expire, requiring users to log in again after a period of time
- `appsettings.Development.json` and `.env.local` are already listed in `.gitignore` — double-check this before pushing any changes

---

## 11. Known Limitations / Future Improvements

- AI features use Groq's free tier, which has rate limits — a paid API plan would be more reliable for production use
- File storage is local to the server — for production, a cloud storage service like Azure Blob Storage would be more appropriate
- No email-to-ticket automation yet (creating a ticket by sending an email)
- No mobile app — the system is responsive but web-only
- No AI ChatBot yet

---

## 12. Acknowledgements

- Developed as part of the IDS Academy Full Stack Web Development Internship
- Supervisor: Suha Mneimneh
- Built over 8 weeks, covering the full software development lifecycle
