using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // Seed Roles
            if (!await context.Roles.AnyAsync())
            {
                context.Roles.AddRange(
                    new Role { Name = "Admin" },
                    new Role { Name = "Manager" },
                    new Role { Name = "Agent" },
                    new Role { Name = "Employee" }
                );
                await context.SaveChangesAsync();
            }

            // Seed Categories
            if (!await context.Categories.AnyAsync())
            {
                context.Categories.AddRange(
                    new Category { Name = "Hardware" },
                    new Category { Name = "Software" },
                    new Category { Name = "Network" },
                    new Category { Name = "Email" },
                    new Category { Name = "Access Request" },
                    new Category { Name = "Other" }
                );
                await context.SaveChangesAsync();
            }

            // Seed Priorities
            if (!await context.Priorities.AnyAsync())
            {
                context.Priorities.AddRange(
                    new Priority { Name = "Low" },
                    new Priority { Name = "Medium" },
                    new Priority { Name = "High" },
                    new Priority { Name = "Critical" }
                );
                await context.SaveChangesAsync();
            }

            // Seed Ticket Statuses
            if (!await context.TicketStatuses.AnyAsync())
            {
                context.TicketStatuses.AddRange(
                    new TicketStatus { Name = "Open" },
                    new TicketStatus { Name = "In Progress" },
                    new TicketStatus { Name = "Pending" },
                    new TicketStatus { Name = "Resolved" },
                    new TicketStatus { Name = "Closed" }
                );
                await context.SaveChangesAsync();
            }

            // Seed Users
            if (!await context.Users.AnyAsync())
            {
                var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");
                var managerRole = await context.Roles.FirstAsync(r => r.Name == "Manager");
                var agentRole = await context.Roles.FirstAsync(r => r.Name == "Agent");
                var employeeRole = await context.Roles.FirstAsync(r => r.Name == "Employee");

                context.Users.AddRange(
                    new User
                    {
                        FirstName = "Admin",
                        LastName = "User",
                        Email = "admin@helpdesk.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                        RoleId = adminRole.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        FirstName = "Manager",
                        LastName = "User",
                        Email = "manager@helpdesk.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager@123"),
                        RoleId = managerRole.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        FirstName = "Agent",
                        LastName = "User",
                        Email = "agent@helpdesk.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Agent@123"),
                        RoleId = agentRole.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        FirstName = "Employee",
                        LastName = "User",
                        Email = "employee@helpdesk.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Employee@123"),
                        RoleId = employeeRole.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                );
                await context.SaveChangesAsync();
            }

            // Seed additional users (idempotent per-email check)
            var agentRoleRef = await context.Roles.FirstAsync(r => r.Name == "Agent");
            var employeeRoleRef = await context.Roles.FirstAsync(r => r.Name == "Employee");
            var managerRoleRef = await context.Roles.FirstAsync(r => r.Name == "Manager");

            var additionalUsers = new[]
            {
                new { Email = "mike@helpdesk.com",  FirstName = "Mike",  LastName = "Johnson", Password = "Agent@123",    RoleId = agentRoleRef.Id },
                new { Email = "sara@helpdesk.com",  FirstName = "Sara",  LastName = "Williams", Password = "Agent@123",   RoleId = agentRoleRef.Id },
                new { Email = "john@helpdesk.com",  FirstName = "John",  LastName = "Smith",    Password = "Agent@123",   RoleId = agentRoleRef.Id },
                new { Email = "alice@helpdesk.com", FirstName = "Alice", LastName = "Brown",    Password = "Employee@123", RoleId = employeeRoleRef.Id },
                new { Email = "bob@helpdesk.com",   FirstName = "Bob",   LastName = "Davis",    Password = "Employee@123", RoleId = employeeRoleRef.Id },
                new { Email = "carol@helpdesk.com", FirstName = "Carol", LastName = "Wilson",   Password = "Employee@123", RoleId = employeeRoleRef.Id },
                new { Email = "david@helpdesk.com", FirstName = "David", LastName = "Lee",      Password = "Manager@123",  RoleId = managerRoleRef.Id }
            };

            foreach (var u in additionalUsers)
            {
                if (!await context.Users.AnyAsync(x => x.Email == u.Email))
                {
                    context.Users.Add(new User
                    {
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(u.Password),
                        RoleId = u.RoleId,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            await context.SaveChangesAsync();

            // Seed Tickets
            if (!await context.Tickets.AnyAsync())
            {
                var catHardware = await context.Categories.FirstAsync(c => c.Name == "Hardware");
                var catSoftware  = await context.Categories.FirstAsync(c => c.Name == "Software");
                var catNetwork   = await context.Categories.FirstAsync(c => c.Name == "Network");
                var catEmail     = await context.Categories.FirstAsync(c => c.Name == "Email");
                var catAccess    = await context.Categories.FirstAsync(c => c.Name == "Access Request");
                var catOther     = await context.Categories.FirstAsync(c => c.Name == "Other");

                var priLow      = await context.Priorities.FirstAsync(p => p.Name == "Low");
                var priMedium   = await context.Priorities.FirstAsync(p => p.Name == "Medium");
                var priHigh     = await context.Priorities.FirstAsync(p => p.Name == "High");
                var priCritical = await context.Priorities.FirstAsync(p => p.Name == "Critical");

                var statOpen       = await context.TicketStatuses.FirstAsync(s => s.Name == "Open");
                var statInProgress = await context.TicketStatuses.FirstAsync(s => s.Name == "In Progress");
                var statPending    = await context.TicketStatuses.FirstAsync(s => s.Name == "Pending");
                var statResolved   = await context.TicketStatuses.FirstAsync(s => s.Name == "Resolved");
                var statClosed     = await context.TicketStatuses.FirstAsync(s => s.Name == "Closed");

                var agent1 = await context.Users.FirstAsync(u => u.Email == "agent@helpdesk.com");
                var agent2 = await context.Users.FirstAsync(u => u.Email == "mike@helpdesk.com");
                var agent3 = await context.Users.FirstAsync(u => u.Email == "sara@helpdesk.com");
                var agent4 = await context.Users.FirstAsync(u => u.Email == "john@helpdesk.com");

                var emp1 = await context.Users.FirstAsync(u => u.Email == "employee@helpdesk.com");
                var emp2 = await context.Users.FirstAsync(u => u.Email == "alice@helpdesk.com");
                var emp3 = await context.Users.FirstAsync(u => u.Email == "bob@helpdesk.com");
                var emp4 = await context.Users.FirstAsync(u => u.Email == "carol@helpdesk.com");

                var now = DateTime.UtcNow;

                context.Tickets.AddRange(
                    new Ticket
                    {
                        ReferenceNumber = "TKT-001",
                        Title = "Laptop not turning on",
                        Description = "My laptop won't power on after the weekend. I pressed the power button but nothing happens.",
                        CategoryId = catHardware.Id, PriorityId = priHigh.Id, TicketStatusId = statOpen.Id,
                        CreatedByUserId = emp1.Id, AssignedToUserId = agent1.Id,
                        IsEscalated = false, CreatedAt = now.AddDays(-28), UpdatedAt = now.AddDays(-27)
                    },
                    new Ticket
                    {
                        ReferenceNumber = "TKT-002",
                        Title = "Cannot access email",
                        Description = "I cannot log into my email account. Getting authentication error since this morning.",
                        CategoryId = catEmail.Id, PriorityId = priMedium.Id, TicketStatusId = statInProgress.Id,
                        CreatedByUserId = emp2.Id, AssignedToUserId = agent2.Id,
                        IsEscalated = false, CreatedAt = now.AddDays(-25), UpdatedAt = now.AddDays(-24)
                    },
                    new Ticket
                    {
                        ReferenceNumber = "TKT-003",
                        Title = "VPN connection issue",
                        Description = "Unable to connect to company VPN from home. Error: connection timeout.",
                        CategoryId = catNetwork.Id, PriorityId = priHigh.Id, TicketStatusId = statInProgress.Id,
                        CreatedByUserId = emp3.Id, AssignedToUserId = agent3.Id,
                        IsEscalated = true, CreatedAt = now.AddDays(-22), UpdatedAt = now.AddDays(-21)
                    },
                    new Ticket
                    {
                        ReferenceNumber = "TKT-004",
                        Title = "Software installation request",
                        Description = "I need Adobe Acrobat Pro installed on my workstation for document review.",
                        CategoryId = catSoftware.Id, PriorityId = priLow.Id, TicketStatusId = statResolved.Id,
                        CreatedByUserId = emp4.Id, AssignedToUserId = agent4.Id,
                        IsEscalated = false, CreatedAt = now.AddDays(-20), UpdatedAt = now.AddDays(-18)
                    },
                    new Ticket
                    {
                        ReferenceNumber = "TKT-005",
                        Title = "Printer not working",
                        Description = "The shared printer on floor 3 is not responding. Print jobs are queued but nothing prints.",
                        CategoryId = catHardware.Id, PriorityId = priMedium.Id, TicketStatusId = statPending.Id,
                        CreatedByUserId = emp1.Id, AssignedToUserId = agent1.Id,
                        IsEscalated = false, CreatedAt = now.AddDays(-18), UpdatedAt = now.AddDays(-17)
                    },
                    new Ticket
                    {
                        ReferenceNumber = "TKT-006",
                        Title = "Password reset needed",
                        Description = "I forgot my Windows login password and am locked out of my computer.",
                        CategoryId = catAccess.Id, PriorityId = priLow.Id, TicketStatusId = statOpen.Id,
                        CreatedByUserId = emp2.Id, AssignedToUserId = null,
                        IsEscalated = false, CreatedAt = now.AddDays(-16), UpdatedAt = now.AddDays(-16)
                    },
                    new Ticket
                    {
                        ReferenceNumber = "TKT-007",
                        Title = "Monitor flickering",
                        Description = "My second monitor keeps flickering every few minutes. Very distracting during work.",
                        CategoryId = catHardware.Id, PriorityId = priMedium.Id, TicketStatusId = statInProgress.Id,
                        CreatedByUserId = emp3.Id, AssignedToUserId = agent2.Id,
                        IsEscalated = false, CreatedAt = now.AddDays(-15), UpdatedAt = now.AddDays(-14)
                    },
                    new Ticket
                    {
                        ReferenceNumber = "TKT-008",
                        Title = "Outlook keeps crashing",
                        Description = "Microsoft Outlook crashes every time I try to open an email with an attachment.",
                        CategoryId = catSoftware.Id, PriorityId = priHigh.Id, TicketStatusId = statOpen.Id,
                        CreatedByUserId = emp4.Id, AssignedToUserId = agent3.Id,
                        IsEscalated = false, CreatedAt = now.AddDays(-13), UpdatedAt = now.AddDays(-13)
                    },
                    new Ticket
                    {
                        ReferenceNumber = "TKT-009",
                        Title = "Cannot connect to WiFi",
                        Description = "Unable to connect to office WiFi network. No wireless networks are being detected.",
                        CategoryId = catNetwork.Id, PriorityId = priCritical.Id, TicketStatusId = statOpen.Id,
                        CreatedByUserId = emp1.Id, AssignedToUserId = agent4.Id,
                        IsEscalated = true, CreatedAt = now.AddDays(-12), UpdatedAt = now.AddDays(-12)
                    },
                    new Ticket
                    {
                        ReferenceNumber = "TKT-010",
                        Title = "New employee setup",
                        Description = "Need workstation setup, email account, and system access for new hire starting Monday.",
                        CategoryId = catAccess.Id, PriorityId = priMedium.Id, TicketStatusId = statInProgress.Id,
                        CreatedByUserId = emp2.Id, AssignedToUserId = agent1.Id,
                        IsEscalated = false, CreatedAt = now.AddDays(-11), UpdatedAt = now.AddDays(-10)
                    },
                    new Ticket
                    {
                        ReferenceNumber = "TKT-011",
                        Title = "Excel file corrupted",
                        Description = "An important Excel spreadsheet is showing as corrupted and cannot be opened.",
                        CategoryId = catSoftware.Id, PriorityId = priHigh.Id, TicketStatusId = statPending.Id,
                        CreatedByUserId = emp3.Id, AssignedToUserId = agent2.Id,
                        IsEscalated = false, CreatedAt = now.AddDays(-10), UpdatedAt = now.AddDays(-9)
                    },
                    new Ticket
                    {
                        ReferenceNumber = "TKT-012",
                        Title = "Phone not receiving calls",
                        Description = "My desk phone is not receiving any incoming calls. Outgoing calls still work.",
                        CategoryId = catHardware.Id, PriorityId = priCritical.Id, TicketStatusId = statOpen.Id,
                        CreatedByUserId = emp4.Id, AssignedToUserId = agent3.Id,
                        IsEscalated = true, CreatedAt = now.AddDays(-9), UpdatedAt = now.AddDays(-9)
                    },
                    new Ticket
                    {
                        ReferenceNumber = "TKT-013",
                        Title = "Database access request",
                        Description = "Requesting read access to the production database for quarterly reporting purposes.",
                        CategoryId = catAccess.Id, PriorityId = priMedium.Id, TicketStatusId = statOpen.Id,
                        CreatedByUserId = emp1.Id, AssignedToUserId = null,
                        IsEscalated = false, CreatedAt = now.AddDays(-8), UpdatedAt = now.AddDays(-8)
                    },
                    new Ticket
                    {
                        ReferenceNumber = "TKT-014",
                        Title = "Antivirus not updating",
                        Description = "The antivirus software on my machine has not updated its definitions in 2 weeks.",
                        CategoryId = catSoftware.Id, PriorityId = priLow.Id, TicketStatusId = statResolved.Id,
                        CreatedByUserId = emp2.Id, AssignedToUserId = agent4.Id,
                        IsEscalated = false, CreatedAt = now.AddDays(-7), UpdatedAt = now.AddDays(-5)
                    },
                    new Ticket
                    {
                        ReferenceNumber = "TKT-015",
                        Title = "Keyboard not working",
                        Description = "Several keys on my keyboard are unresponsive. Tried restarting but issue persists.",
                        CategoryId = catHardware.Id, PriorityId = priLow.Id, TicketStatusId = statClosed.Id,
                        CreatedByUserId = emp3.Id, AssignedToUserId = agent1.Id,
                        IsEscalated = false, CreatedAt = now.AddDays(-6), UpdatedAt = now.AddDays(-4)
                    },
                    new Ticket
                    {
                        ReferenceNumber = "TKT-016",
                        Title = "Email attachment limit",
                        Description = "Cannot send email attachments larger than 5MB. Need this limit increased for project files.",
                        CategoryId = catEmail.Id, PriorityId = priLow.Id, TicketStatusId = statResolved.Id,
                        CreatedByUserId = emp4.Id, AssignedToUserId = agent2.Id,
                        IsEscalated = false, CreatedAt = now.AddDays(-5), UpdatedAt = now.AddDays(-3)
                    },
                    new Ticket
                    {
                        ReferenceNumber = "TKT-017",
                        Title = "Slow internet speed",
                        Description = "Internet speed is very slow in the east wing of the building. Affecting multiple users.",
                        CategoryId = catNetwork.Id, PriorityId = priMedium.Id, TicketStatusId = statInProgress.Id,
                        CreatedByUserId = emp1.Id, AssignedToUserId = agent3.Id,
                        IsEscalated = false, CreatedAt = now.AddDays(-4), UpdatedAt = now.AddDays(-3)
                    },
                    new Ticket
                    {
                        ReferenceNumber = "TKT-018",
                        Title = "Teams not connecting",
                        Description = "Microsoft Teams is unable to connect. Shows 'We're sorry, we've run into an issue' error.",
                        CategoryId = catSoftware.Id, PriorityId = priHigh.Id, TicketStatusId = statOpen.Id,
                        CreatedByUserId = emp2.Id, AssignedToUserId = agent4.Id,
                        IsEscalated = false, CreatedAt = now.AddDays(-3), UpdatedAt = now.AddDays(-3)
                    },
                    new Ticket
                    {
                        ReferenceNumber = "TKT-019",
                        Title = "Server room access",
                        Description = "Need physical access to server room for infrastructure maintenance. Urgent approval needed.",
                        CategoryId = catAccess.Id, PriorityId = priCritical.Id, TicketStatusId = statOpen.Id,
                        CreatedByUserId = emp3.Id, AssignedToUserId = null,
                        IsEscalated = true, CreatedAt = now.AddDays(-2), UpdatedAt = now.AddDays(-2)
                    },
                    new Ticket
                    {
                        ReferenceNumber = "TKT-020",
                        Title = "Backup failure alert",
                        Description = "Automated backup system is reporting failures for the last 3 days. Immediate attention required.",
                        CategoryId = catOther.Id, PriorityId = priCritical.Id, TicketStatusId = statOpen.Id,
                        CreatedByUserId = emp4.Id, AssignedToUserId = agent1.Id,
                        IsEscalated = true, CreatedAt = now.AddDays(-1), UpdatedAt = now.AddDays(-1)
                    }
                );
                await context.SaveChangesAsync();
            }

            // Seed Comments
            if (!await context.TicketComments.AnyAsync())
            {
                var t1  = await context.Tickets.FirstAsync(t => t.ReferenceNumber == "TKT-001");
                var t2  = await context.Tickets.FirstAsync(t => t.ReferenceNumber == "TKT-002");
                var t3  = await context.Tickets.FirstAsync(t => t.ReferenceNumber == "TKT-003");
                var t5  = await context.Tickets.FirstAsync(t => t.ReferenceNumber == "TKT-005");
                var t9  = await context.Tickets.FirstAsync(t => t.ReferenceNumber == "TKT-009");
                var t11 = await context.Tickets.FirstAsync(t => t.ReferenceNumber == "TKT-011");
                var t20 = await context.Tickets.FirstAsync(t => t.ReferenceNumber == "TKT-020");

                var agent1 = await context.Users.FirstAsync(u => u.Email == "agent@helpdesk.com");
                var agent2 = await context.Users.FirstAsync(u => u.Email == "mike@helpdesk.com");
                var agent3 = await context.Users.FirstAsync(u => u.Email == "sara@helpdesk.com");
                var agent4 = await context.Users.FirstAsync(u => u.Email == "john@helpdesk.com");

                context.TicketComments.AddRange(
                    new TicketComment { TicketId = t1.Id,  UserId = agent1.Id, Content = "Looking into this issue. Will check the power supply and battery.", CreatedAt = DateTime.UtcNow.AddDays(-27) },
                    new TicketComment { TicketId = t2.Id,  UserId = agent2.Id, Content = "Contacted vendor for support. Waiting for response.", CreatedAt = DateTime.UtcNow.AddDays(-24) },
                    new TicketComment { TicketId = t3.Id,  UserId = agent3.Id, Content = "Issue reproduced, working on fix. VPN gateway configuration needs update.", CreatedAt = DateTime.UtcNow.AddDays(-21) },
                    new TicketComment { TicketId = t3.Id,  UserId = agent3.Id, Content = "Escalated to senior network team for further investigation.", CreatedAt = DateTime.UtcNow.AddDays(-20) },
                    new TicketComment { TicketId = t5.Id,  UserId = agent1.Id, Content = "Waiting for user to confirm after replacing printer cartridge.", CreatedAt = DateTime.UtcNow.AddDays(-17) },
                    new TicketComment { TicketId = t9.Id,  UserId = agent4.Id, Content = "Checked access point configuration. Issue may be driver-related.", CreatedAt = DateTime.UtcNow.AddDays(-12) },
                    new TicketComment { TicketId = t11.Id, UserId = agent2.Id, Content = "Attempting file recovery using Excel built-in repair tool.", CreatedAt = DateTime.UtcNow.AddDays(-9) },
                    new TicketComment { TicketId = t11.Id, UserId = agent2.Id, Content = "File recovery partially successful. Waiting for user to verify data integrity.", CreatedAt = DateTime.UtcNow.AddDays(-8) },
                    new TicketComment { TicketId = t20.Id, UserId = agent1.Id, Content = "Escalated to senior team. Backup failure affecting critical systems.", CreatedAt = DateTime.UtcNow.AddDays(-1) },
                    new TicketComment { TicketId = t20.Id, UserId = agent1.Id, Content = "Storage volume appears full. Investigating storage allocation.", CreatedAt = DateTime.UtcNow.AddHours(-6) }
                );
                await context.SaveChangesAsync();
            }

            // Seed Notifications
            if (!await context.Notifications.AnyAsync())
            {
                var agent1  = await context.Users.FirstAsync(u => u.Email == "agent@helpdesk.com");
                var agent2  = await context.Users.FirstAsync(u => u.Email == "mike@helpdesk.com");
                var emp1    = await context.Users.FirstAsync(u => u.Email == "employee@helpdesk.com");
                var emp2    = await context.Users.FirstAsync(u => u.Email == "alice@helpdesk.com");
                var emp3    = await context.Users.FirstAsync(u => u.Email == "bob@helpdesk.com");
                var emp4    = await context.Users.FirstAsync(u => u.Email == "carol@helpdesk.com");
                var manager = await context.Users.FirstAsync(u => u.Email == "manager@helpdesk.com");

                context.Notifications.AddRange(
                    new Notification { UserId = emp1.Id,    Message = "Your ticket TKT-001 has been updated.",                   IsRead = false, CreatedAt = DateTime.UtcNow.AddDays(-27) },
                    new Notification { UserId = agent1.Id,  Message = "New ticket TKT-001 has been assigned to you.",            IsRead = true,  CreatedAt = DateTime.UtcNow.AddDays(-27) },
                    new Notification { UserId = emp2.Id,    Message = "Your ticket TKT-002 is now In Progress.",                 IsRead = false, CreatedAt = DateTime.UtcNow.AddDays(-24) },
                    new Notification { UserId = agent2.Id,  Message = "New ticket TKT-002 has been assigned to you.",            IsRead = true,  CreatedAt = DateTime.UtcNow.AddDays(-24) },
                    new Notification { UserId = emp4.Id,    Message = "Your ticket TKT-004 has been resolved.",                  IsRead = true,  CreatedAt = DateTime.UtcNow.AddDays(-18) },
                    new Notification { UserId = emp1.Id,    Message = "Ticket TKT-005 is pending your confirmation.",            IsRead = false, CreatedAt = DateTime.UtcNow.AddDays(-17) },
                    new Notification { UserId = emp3.Id,    Message = "Your ticket TKT-003 has been escalated.",                 IsRead = false, CreatedAt = DateTime.UtcNow.AddDays(-20) },
                    new Notification { UserId = manager.Id, Message = "Ticket TKT-009 has been escalated and requires attention.", IsRead = false, CreatedAt = DateTime.UtcNow.AddDays(-12) },
                    new Notification { UserId = emp2.Id,    Message = "Your ticket TKT-014 has been resolved.",                  IsRead = true,  CreatedAt = DateTime.UtcNow.AddDays(-5)  },
                    new Notification { UserId = emp3.Id,    Message = "Your ticket TKT-015 has been closed.",                    IsRead = true,  CreatedAt = DateTime.UtcNow.AddDays(-4)  },
                    new Notification { UserId = emp4.Id,    Message = "Your ticket TKT-016 has been resolved.",                  IsRead = true,  CreatedAt = DateTime.UtcNow.AddDays(-3)  },
                    new Notification { UserId = manager.Id, Message = "Ticket TKT-012 has been escalated.",                      IsRead = false, CreatedAt = DateTime.UtcNow.AddDays(-9)  },
                    new Notification { UserId = emp4.Id,    Message = "Your ticket TKT-020 has been escalated to senior team.",  IsRead = false, CreatedAt = DateTime.UtcNow.AddDays(-1)  },
                    new Notification { UserId = manager.Id, Message = "Ticket TKT-019 requires urgent approval.",                IsRead = false, CreatedAt = DateTime.UtcNow.AddDays(-2)  },
                    new Notification { UserId = emp1.Id,    Message = "New comment added to your ticket TKT-001.",               IsRead = false, CreatedAt = DateTime.UtcNow.AddDays(-27) }
                );
                await context.SaveChangesAsync();
            }

            // Seed Activity Logs
            if (!await context.ActivityLogs.AnyAsync())
            {
                var admin   = await context.Users.FirstAsync(u => u.Email == "admin@helpdesk.com");
                var agent1  = await context.Users.FirstAsync(u => u.Email == "agent@helpdesk.com");
                var agent2  = await context.Users.FirstAsync(u => u.Email == "mike@helpdesk.com");
                var agent3  = await context.Users.FirstAsync(u => u.Email == "sara@helpdesk.com");
                var emp1    = await context.Users.FirstAsync(u => u.Email == "employee@helpdesk.com");
                var emp2    = await context.Users.FirstAsync(u => u.Email == "alice@helpdesk.com");
                var manager = await context.Users.FirstAsync(u => u.Email == "manager@helpdesk.com");

                context.ActivityLogs.AddRange(
                    new ActivityLog { UserId = emp1.Id,    Action = "Ticket Created",       Details = "Ticket TKT-001 created: Laptop not turning on",             LoggedAt = DateTime.UtcNow.AddDays(-28) },
                    new ActivityLog { UserId = agent1.Id,  Action = "Ticket Assigned",      Details = "Ticket TKT-001 assigned to Agent User",                     LoggedAt = DateTime.UtcNow.AddDays(-27) },
                    new ActivityLog { UserId = agent2.Id,  Action = "Ticket Status Updated",Details = "Ticket TKT-002 status changed to In Progress",              LoggedAt = DateTime.UtcNow.AddDays(-24) },
                    new ActivityLog { UserId = agent3.Id,  Action = "Ticket Escalated",     Details = "Ticket TKT-003 escalated due to network impact",             LoggedAt = DateTime.UtcNow.AddDays(-21) },
                    new ActivityLog { UserId = agent1.Id,  Action = "Ticket Resolved",      Details = "Ticket TKT-004 marked as resolved",                         LoggedAt = DateTime.UtcNow.AddDays(-18) },
                    new ActivityLog { UserId = agent1.Id,  Action = "Comment Added",        Details = "Comment added to TKT-005 by Agent User",                    LoggedAt = DateTime.UtcNow.AddDays(-17) },
                    new ActivityLog { UserId = admin.Id,   Action = "User Logged In",       Details = "Admin User logged in successfully",                          LoggedAt = DateTime.UtcNow.AddDays(-10) },
                    new ActivityLog { UserId = manager.Id, Action = "User Logged In",       Details = "Manager User logged in successfully",                        LoggedAt = DateTime.UtcNow.AddDays(-5)  },
                    new ActivityLog { UserId = emp2.Id,    Action = "Ticket Created",       Details = "Ticket TKT-018 created: Teams not connecting",               LoggedAt = DateTime.UtcNow.AddDays(-3)  },
                    new ActivityLog { UserId = agent1.Id,  Action = "Ticket Escalated",     Details = "Ticket TKT-020 escalated: Backup failure alert",             LoggedAt = DateTime.UtcNow.AddDays(-1)  }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
