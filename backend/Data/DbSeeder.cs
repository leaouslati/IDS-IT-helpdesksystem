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
        }
    }
}