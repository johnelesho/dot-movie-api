using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace DotMovie.Entities;

public class Rating
{
    public int Id { get; set; }
    [Range(1,5)]
    public int Rate { get; set; }

    public int MovieId { get; set; }
    public int UserId { get; set; }

    public Movie Movie { get; set; }
    public IdentityUser User { get; set; }
}