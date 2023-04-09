namespace DotMovie.Dtos;

public class MoviePostGetDto
{
    public List<GenreDto> Genres { get; set; }
    public List<MovieTheaterDto> MovieTheaters { get; set; }
}