using EmployeeSupportAgent.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeSupportAgent.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<ITTicket> ITTickets => Set<ITTicket>();
    public DbSet<Payslip> Payslips => Set<Payslip>();
    public DbSet<Reimbursement> Reimbursements => Set<Reimbursement>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ----- Decimal precision -----
        modelBuilder.Entity<Reimbursement>()
            .Property(r => r.Amount)
            .HasColumnType("decimal(18,2)");

        // ----- Unique indexes -----
        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.EmployeeCode)
            .IsUnique()
            .HasDatabaseName("UX_Employees_EmployeeCode");

        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.Email)
            .IsUnique()
            .HasDatabaseName("UX_Employees_Email");

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique()
            .HasDatabaseName("UX_Users_Username");

        // Each Employee has at most one login row.
        modelBuilder.Entity<User>()
            .HasIndex(u => u.EmployeeId)
            .IsUnique()
            .HasDatabaseName("UX_Users_EmployeeId");

        // ----- Foreign keys -----
        modelBuilder.Entity<User>()
            .HasOne<Employee>()
            .WithMany()
            .HasForeignKey(u => u.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        // ----- Performance indexes -----
        modelBuilder.Entity<LeaveRequest>()
            .HasIndex(l => l.EmployeeId)
            .HasDatabaseName("IX_LeaveRequests_EmployeeId");
        modelBuilder.Entity<LeaveRequest>()
            .HasIndex(l => l.Status)
            .HasDatabaseName("IX_LeaveRequests_Status");

        modelBuilder.Entity<ITTicket>()
            .HasIndex(t => t.EmployeeId)
            .HasDatabaseName("IX_ITTickets_EmployeeId");
        modelBuilder.Entity<ITTicket>()
            .HasIndex(t => t.Status)
            .HasDatabaseName("IX_ITTickets_Status");

        modelBuilder.Entity<Payslip>()
            .HasIndex(p => p.EmployeeId)
            .HasDatabaseName("IX_Payslips_EmployeeId");

        modelBuilder.Entity<Reimbursement>()
            .HasIndex(r => r.EmployeeId)
            .HasDatabaseName("IX_Reimbursements_EmployeeId");
        modelBuilder.Entity<Reimbursement>()
            .HasIndex(r => r.Status)
            .HasDatabaseName("IX_Reimbursements_Status");

        modelBuilder.Entity<ChatMessage>()
            .HasIndex(m => m.EmployeeId)
            .HasDatabaseName("IX_ChatMessages_EmployeeId");

        base.OnModelCreating(modelBuilder);
    }
}