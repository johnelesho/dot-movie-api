using System.ComponentModel.DataAnnotations;

namespace DotMovie.Dtos;

public class ActorCreationDto
{
    [Required]
    [StringLength(120)]
    public string Name { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Biography { get; set; }
    public IFormFile Picture { get; set; }
}