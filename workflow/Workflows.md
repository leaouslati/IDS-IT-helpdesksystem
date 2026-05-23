# IT Help Desk & Ticketing Management System
## System Workflows

---

## 1. User Login

- User opens the app
- Enters email and password
- System checks credentials
- If wrong → show error message
- If correct → redirect to dashboard based on role

---

## 2. Create Ticket

- Employee clicks New Ticket
- Fills in title, description, category, priority
- Optionally uploads attachments
- Clicks Submit
- System generates ticket number
- Ticket status set to Open
- Notification sent to IT agents
- Activity logged

---

## 3. Ticket Assignment

- Agent or Admin opens ticket
- Selects agent from Assign To dropdown
- Clicks Save
- Ticket AssignedTo updated
- Notification sent to assigned agent
- Activity logged

---

## 4. Ticket Resolution

- Agent opens assigned ticket
- Works on the issue
- Changes status to In Progress
- Adds comments if needed
- Changes status to Resolved
- System records ResolvedAt timestamp
- Notification sent to employee
- Activity logged

---

## 5. Ticket Escalation

- Agent or Admin marks ticket as escalated
- IsEscalated flag set to true
- Notification sent to Manager and Admin
- Activity logged

---

## 6. Admin Creates User

- Admin goes to Admin Settings
- Clicks Add New User
- Fills in name, email, role
- System creates account
- Welcome notification sent to new user
- Activity logged

---
