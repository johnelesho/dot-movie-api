using System.ComponentModel.DataAnnotations;
using DotMovie.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace DotMovie.Entities;

public class Genre
{
    
        public int Id { get; set; }
        [Required(ErrorMessage = "The field with name {0} is required")]
        [StringLength(50)]
        [FirstLetterUppercase]
        public string Name { get; set; }
        
}