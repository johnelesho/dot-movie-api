namespace DotMovie.Dtos;

public class MoviePutGetDto
{
    public MovieDto Movie { get; set; }
    public List<GenreDto> SelectedGenres { get; set; }
    public List<GenreDto> NonSelectedGenres { get; set; }
    public List<MovieTheaterDto> SelectedMovieTheaters { get; set; }
    public List<MovieTheaterDto> NonSelectedMovieTheaters { get; set; }
    public List<ActorsMovieDto> Actors { get; set; }
}

