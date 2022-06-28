using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeyWimey.Model;
public class TimeCode
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

#nullable disable
    public TimeCodeSystem System { get; set; }

    public string Code { get; set; }
    public string Description{ get; set; }

    public int SystemId { get; set; }

    public ICollection<TimeActivity> Activities { get; set; }
#nullable enable
}
