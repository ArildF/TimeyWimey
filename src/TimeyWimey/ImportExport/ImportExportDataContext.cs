using Microsoft.EntityFrameworkCore;

namespace TimeyWimey.ImportExport;

public class ImportExportDataContext : DbContext
{
    public ImportExportDataContext(DbContextOptions<ImportExportDataContext> options) : base(options)
        { }
#nullable disable
    public DbSet<DayExport> Days { get; set; }
    public DbSet<TimeActivityExport> Activities { get; set; }
    public DbSet<TimeEntryExport> Entries { get; set; }
    public DbSet<TimeCodeExport> TimeCodes { get; set; }
    public DbSet<TimeCodeSystemExport> CodeSystems { get; set; }

    public DbSet<TimeActivityTimeCodeExport> TimeActivityTimeCode { get; set; }
#nullable enable

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<DayExport>().HasKey(e => e.Id);
        b.Entity<TimeActivityExport>().HasKey(e => e.Id);
        b.Entity<TimeEntryExport>().HasKey(e => e.Id);
        b.Entity<TimeCodeExport>().HasKey(e => e.Id);
        b.Entity<TimeCodeSystemExport>().HasKey(e => e.Id);
        b.Entity<TimeActivityTimeCodeExport>().HasKey(e => new { e.ActivitiesId, e.TimeCodesId });

    }
}