using Microsoft.EntityFrameworkCore;
using TimeyWimey.Model;

namespace TimeyWimey.Data;

public class WimeyDataContext : DbContext
{
    public WimeyDataContext(DbContextOptions options) : base(options)
    {
        
    }
#nullable disable
    public DbSet<Day> Days { get; set; }
    public DbSet<TimeActivity> Activities { get; set; }
    public DbSet<TimeEntry> Entries { get; set; }
    public DbSet<TimeCode> TimeCodes { get; set; }
    public DbSet<TimeCodeSystem> CodeSystems { get; set; }
#nullable enable
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Day>().HasIndex(d => d.Date);
        b.Entity<Day>().HasKey(d => d.Id);
        b.Entity<TimeEntry>().HasKey(te => te.Id);
    }
}