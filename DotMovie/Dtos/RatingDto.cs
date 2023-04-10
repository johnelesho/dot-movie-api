using System.ComponentModel.DataAnnotations;

namespace DotMovie.Dtos;

public class RatingDto
{
    public int Id { get; set; }
    [Range(1,5)]
    public int Rate { get; set; }
    public int MovieId { get; set; }
 
   
}