# IDS IT Help Desk & Ticketing Management System

A modern web-based IT Help Desk and Ticketing Management System built as part of the IDS Academy Online Internship Program.

## Tech Stack

- **Frontend:** Vue.js 3 + Tailwind CSS
- **Backend:** ASP.NET Core Web API (C#)
- **Database:** SQL Server 2025 Express
- **Authentication:** JWT
- **Version Control:** Git & GitHub


## System Users & Roles
 
| Role | Description |
|---|---|
| Admin | Full system access, manages users and settings |
| IT Support Agent | Manages and resolves tickets |
| Employee | Creates and tracks their own tickets |
| Manager | Monitors team tickets and reports |

## Ticket Workflow

1. Employee creates a ticket — it goes to their department manager, unassigned
2. Manager reviews unassigned tickets and assigns one to an available agent
3. Agent sees the ticket as Open and clicks **Start** to begin working (status → In Progress)
4. Agent updates the status as work progresses: Pending or Resolved
5. Employee gets notified when their ticket is Resolved
6. No manager approval is needed to resolve a ticket — the agent resolves it directly
