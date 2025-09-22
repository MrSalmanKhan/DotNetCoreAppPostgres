using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<AppUser> Users { get; set; }
    public DbSet<AppRole> Roles { get; set; }
    public DbSet<AppUserRole> UserRoles { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });
        modelBuilder.Entity<ChatMessage>()
            .HasOne(cm => cm.AppUser)
            .WithMany(u => u.ChatMessages)
            .HasForeignKey(cm => cm.AppUserId)
            .OnDelete(DeleteBehavior.Cascade); // Delete messages if user is deleted

        // Indexes for performance
        modelBuilder.Entity<AppUser>()
            .HasIndex(u => u.Name);

        modelBuilder.Entity<ChatMessage>()
            .HasIndex(cm => cm.Timestamp); // index on Timestamp

        modelBuilder.Entity<ChatMessage>()
            .HasIndex(cm => cm.AppUserId); // index on AppUserId

        base.OnModelCreating(modelBuilder);
    }
}