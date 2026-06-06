using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Opti_Sec_Backend.Entities;

namespace Opti_Sec_Backend.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options
    , IHttpContextAccessor httpContextAccessor)
    : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public DbSet<Client> Clients { get; set; }

    public DbSet<Member> Members { get; set; }

    public DbSet<AccessLog> AccessLogs { get; set; }
    public DbSet<Gate> Gates { get; set; }

    public DbSet<GateSession> GateSessions { get; set; }
    public DbSet<PasswordAttempt> PasswordAttempts { get; set; }
    public DbSet<EmergencyEvent> EmergencyEvents { get; set; }
    public DbSet<DeviceCommand> DeviceCommands { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<AIValidationLog> AIValidationLogs { get; set; }
    public DbSet<FingerprintValidationLog> FingerprintValidationLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());


        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasOne(typeof(ApplicationUser), "CreatedBy")
                    .WithMany()
                    .HasForeignKey("CreatedById")
                    .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity(entityType.ClrType)
                    .HasOne(typeof(ApplicationUser), "UpdatedBy")
                    .WithMany()
                    .HasForeignKey("UpdatedById")
                    .OnDelete(DeleteBehavior.Restrict);
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    //// this function return int to show numbers of rows that afftected in DB 
    //public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    //{
    //    // here will return the all entities that track with Autitable 
    //    var enteries = ChangeTracker.Entries<AuditableEntity>();

    //    foreach (var entityEntry in enteries)
    //    {
    //        // to bring on the user ID 
    //        var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    //        if (entityEntry.State == EntityState.Added)
    //        {
    //            entityEntry.Property(x => x.CreatedById).CurrentValue = currentUserId;
    //        }
    //        if (entityEntry.State == EntityState.Modified)
    //        {
    //            entityEntry.Property(x => x.UpdatedById).CurrentValue = currentUserId;
    //            entityEntry.Property(x => x.UpdatedOn).CurrentValue = DateTime.UtcNow;

    //        }

    //    }
    //    return base.SaveChangesAsync(cancellationToken);
    //}
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<AuditableEntity>();

        var currentUserId = _httpContextAccessor.HttpContext?.User?
            .FindFirstValue(ClaimTypes.NameIdentifier);

        foreach (var entityEntry in entries)
        {
            if (string.IsNullOrEmpty(currentUserId))
                continue;

            if (entityEntry.State == EntityState.Added)
            {
                entityEntry.Property(x => x.CreatedById).CurrentValue = currentUserId;
                entityEntry.Property(x => x.CreatedOn).CurrentValue = DateTime.UtcNow;
            }

            if (entityEntry.State == EntityState.Modified)
            {
                entityEntry.Property(x => x.UpdatedById).CurrentValue = currentUserId;
                entityEntry.Property(x => x.UpdatedOn).CurrentValue = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.ConfigureWarnings(w =>
            w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

}
