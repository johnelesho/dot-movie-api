using DotMovie.Dtos;
using DotMovie.Entities;

namespace DotMovie.Helpers;

public static class DtoExtensions
{
    public static GenreDto toGenreDto(this Genre genre)
    {
        return new GenreDto()
        {
            Id = genre.Id,
            Name = genre.Name
        };
    }
    
    public static Genre toGenreEntity(this GenreCreationDto genre)
    {
        return new Genre()
        {
            Name = genre.Name
        };
    }
}