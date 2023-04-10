using System.ComponentModel.DataAnnotations;

namespace DotMovie.Dtos;

public class UserCreate
{
    [Required]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
}