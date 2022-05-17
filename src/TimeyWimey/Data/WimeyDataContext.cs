using Microsoft.EntityFrameworkCore;
using TimeyWimey.Model;

namespace TimeyWimey.Data;

public class WimeyDataContext : DbContext
{
    public WimeyDataContext(DbContextOptions options) : base(options)
    {
        
    }
    public DbSet<Day>? Days { get; set; }
    public DbSet<TimeActivity> Activities { get; set; }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Day>().HasIndex(d => d.Date);
        b.Entity<Day>().HasKey(d => d.Id);
        b.Entity<TimeEntry>().HasKey(te => te.Id);
    }
}