using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeyWimey.Model;

public class TimeActivity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int Id { get; set; }

    public string? Name { get; set; }

    public DateTime Created { get; set; } = DateTime.Now;

    public string? Color { get; set; }

    public ICollection<TimeCode> TimeCode { get; set; }

}