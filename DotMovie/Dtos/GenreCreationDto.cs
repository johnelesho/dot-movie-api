using System.ComponentModel.DataAnnotations;
using DotMovie.Entities;

namespace DotMovie.Dtos;

public class GenreCreationDto
{
    [Required(ErrorMessage = "The field with name {0} is required")]
    [StringLength(50)]
    [FirstLetterUppercase]
    public string Name { get; set; }
}