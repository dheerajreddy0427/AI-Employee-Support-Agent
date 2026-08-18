using EmployeeSupportAgent.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeSupportAgent.API.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, ILogger logger)
    {
        // Check by EmployeeCode so the seed is portable across providers
        if (await db.Employees.AnyAsync(e => e.EmployeeCode == "EMP001"))
        {
            logger.LogInformation("Seed: data already present, skipping.");
            return;
        }

        logger.LogInformation("Seed: inserting demo data...");

        var now = DateTime.UtcNow;

        var admin = new Employee
        {
            EmployeeCode = "EMP001",
            FullName = "Alex Carter",
            Department = "Administration",
            Email = "admin@company.com",
            LeaveBalance = 25,
            Role = "Admin",
            CreatedAt = now,
            UpdatedAt = now
        };
        var manager = new Employee
        {
            EmployeeCode = "EMP002",
            FullName = "Morgan Lee",
            Department = "Engineering",
            Email = "manager@company.com",
            LeaveBalance = 20,
            Role = "Manager",
            CreatedAt = now,
            UpdatedAt = now
        };
        var hr = new Employee
        {
            EmployeeCode = "EMP003",
            FullName = "Riley Patel",
            Department = "Human Resources",
            Email = "hr@company.com",
            LeaveBalance = 20,
            Role = "HR",
            CreatedAt = now,
            UpdatedAt = now
        };
        var employee = new Employee
        {
            EmployeeCode = "EMP004",
            FullName = "Jamie Singh",
            Department = "IT",
            Email = "employee@company.com",
            LeaveBalance = 15,
            Role = "Employee",
            CreatedAt = now,
            UpdatedAt = now
        };
        var newHire = new Employee
        {
            EmployeeCode = "EMP005",
            FullName = "Quinn Reyes",
            Department = "Sales",
            Email = "newhire@company.com",
            LeaveBalance = 10,
            Role = "Employee",
            CreatedAt = now,
            UpdatedAt = now
        };

        await db.Employees.AddRangeAsync(admin, manager, hr, employee, newHire);
        await db.SaveChangesAsync();

        // BCrypt the seed password once so every demo user can log in
        var hash = BCrypt.Net.BCrypt.HashPassword("password123", workFactor: 11);
        await db.Users.AddRangeAsync(
            new User { Username = "EMP001", PasswordHash = hash, EmployeeId = admin.Id, IsActive = true, MustChangePassword = false, CreatedAt = now, UpdatedAt = now },
            new User { Username = "EMP002", PasswordHash = hash, EmployeeId = manager.Id, IsActive = true, MustChangePassword = false, CreatedAt = now, UpdatedAt = now },
            new User { Username = "EMP003", PasswordHash = hash, EmployeeId = hr.Id, IsActive = true, MustChangePassword = false, CreatedAt = now, UpdatedAt = now },
            new User { Username = "EMP004", PasswordHash = hash, EmployeeId = employee.Id, IsActive = true, MustChangePassword = false, CreatedAt = now, UpdatedAt = now },
            // EMP005 must change their password on first sign-in.
            new User { Username = "EMP005", PasswordHash = hash, EmployeeId = newHire.Id, IsActive = true, MustChangePassword = true, CreatedAt = now, UpdatedAt = now }
        );
        await db.SaveChangesAsync();

        // Payslips for the Employee user
        await db.Payslips.AddRangeAsync(
            new Payslip
            {
                EmployeeId = employee.Id,
                MonthYear = "July 2026",
                FileName = "payslip-jul-2026.pdf",
                FileUrl = "https://example.com/payslips/EMP004-2026-07.pdf",
                UploadedDate = now.AddDays(-30),
                CreatedAt = now,
                UpdatedAt = now
            },
            new Payslip
            {
                EmployeeId = employee.Id,
                MonthYear = "June 2026",
                FileName = "payslip-jun-2026.pdf",
                FileUrl = "https://example.com/payslips/EMP004-2026-06.pdf",
                UploadedDate = now.AddDays(-60),
                CreatedAt = now,
                UpdatedAt = now
            }
        );

        // Sample open IT ticket
        await db.ITTickets.AddAsync(new ITTicket
        {
            EmployeeId = employee.Id,
            IssueTitle = "VPN connection drops",
            Description = "VPN disconnects every 30 minutes when working from home.",
            Status = "Open",
            CreatedDate = now.AddDays(-3),
            UpdatedAt = now
        });

        // Sample pending reimbursement
        await db.Reimbursements.AddAsync(new Reimbursement
        {
            EmployeeId = employee.Id,
            Amount = 128.50m,
            Description = "Client lunch meeting",
            Status = "Pending",
            SubmittedDate = now.AddDays(-2)
        });

        // Sample approved leave request
        await db.LeaveRequests.AddAsync(new LeaveRequest
        {
            EmployeeId = employee.Id,
            StartDate = DateTime.Today.AddDays(-30),
            EndDate = DateTime.Today.AddDays(-28),
            Reason = "Family function",
            Status = "Approved",
            ApprovedBy = manager.Id,
            ApprovedDate = now.AddDays(-25),
            Remarks = "Approved",
            CreatedAt = now.AddDays(-31),
            UpdatedAt = now.AddDays(-25)
        });

        // Sample chat history
        await db.ChatMessages.AddRangeAsync(
            new ChatMessage
            {
                EmployeeId = employee.Id,
                Sender = "User",
                MessageText = "How many leaves do I have?",
                CreatedAt = now.AddDays(-5)
            },
            new ChatMessage
            {
                EmployeeId = employee.Id,
                Sender = "Agent",
                MessageText = "You have 15 leave days remaining.",
                CreatedAt = now.AddDays(-5).AddSeconds(1)
            }
        );

        await db.SaveChangesAsync();
        logger.LogInformation("Seed: complete.");
    }
}