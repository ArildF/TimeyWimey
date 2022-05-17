using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace TimeyWimey.Model;

[DebuggerDisplay("{Start.Hour}:{Start.Minute}-{End.Hour}:{End.Minute}")]
public class TimeEntry
{
    public TimeOnly Start { get; set; }
    public TimeOnly End { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }

    public TimeActivity? Activity { get; set; }

    public string? Color { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]

    public int Id { get; set; }
}