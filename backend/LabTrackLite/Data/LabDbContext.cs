using Microsoft.EntityFrameworkCore;

namespace LabTrackLite;

public class LabDbContext : DbContext
{
 public LabDbContext(DbContextOptions<LabDbContext> o) : base(o) {}
 public DbSet<Asset> Assets => Set<Asset>();
 public DbSet<Ticket> Tickets => Set<Ticket>();
 public DbSet<User> Users => Set<User>();
public DbSet<Comment> Comments => Set<Comment>();
public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

}
