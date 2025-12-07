using microservices_project.Core.Domain;
using microservices_project.Core.Domain.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace microservices_project.Infrastructure.DataStorage;

public class ServerDbContext : DbContext
{
    public DbSet<Media> Medias { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    public ServerDbContext(DbContextOptions<ServerDbContext> options) : base(options) {}
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Port=5332;Database=myapp;Username=postgres;Password=password");
    }
}