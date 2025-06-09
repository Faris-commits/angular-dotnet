using System.ComponentModel.DataAnnotations;

namespace API.Entities;

public class Tag
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = null!;

    public ICollection<PhotoTag> PhotoTags { get; set; } = new List<PhotoTag>();
}
