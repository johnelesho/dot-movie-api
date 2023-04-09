using DotMovie.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace DotMovie.Dtos;

public class MovieCreationDto
{
    public string Title { get; set; }
    public string Summary { get; set; }
    public string Trailer { get; set; }
    public bool InTheaters { get; set; }
    public DateTime ReleaseDate { get; set; }
    public IFormFile Poster { get; set; }
    [ModelBinder(BinderType = typeof(TypeBinder<List<int>>))]
    public List<int> GenresIds { get; set; }
    [ModelBinder(BinderType = typeof(TypeBinder<List<int>>))]
    public List<int> MovieTheatersIds { get; set; }
    [ModelBinder(BinderType = typeof(TypeBinder<List<MoviesActorsCreationDto>>))]
    public List<MoviesActorsCreationDto> Actors { get; set; }

}