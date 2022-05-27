using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeyWimey.Model;

public class TimeCodeSystem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

#nullable disable
    public ICollection<TimeCode> TimeCodes { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }
#nullable enable
}