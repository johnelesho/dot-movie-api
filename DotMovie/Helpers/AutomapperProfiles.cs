using AutoMapper;
using DotMovie.Dtos;
using DotMovie.Entities;
using NetTopologySuite.Geometries;

namespace DotMovie.Helpers;

public class AutomapperProfiles : Profile
{
    public AutomapperProfiles(GeometryFactory geometryFactory)
    {
        CreateMap<GenreDto, Genre>().ReverseMap();
        CreateMap<GenreCreationDto, Genre>();
        
        CreateMap<ActorDto, Actor>().ReverseMap();
        CreateMap<ActorCreationDto, Actor>()
            .ForMember(x => x.Picture, options => options.Ignore());

        CreateMap<MovieTheaterCreationDto, MovieTheater>()
            .ForMember(x => x.Location, Dto => Dto.MapFrom(prop => geometryFactory.CreatePoint(new Coordinate(prop.Longitude, prop.Latitude))));
        CreateMap<MovieTheater, MovieTheaterDto>()
            .ForMember(x => x.Latitude,Dto=>Dto.MapFrom(pro => pro.Location.Y))
            .ForMember(x => x.Longitude, Dto => Dto.MapFrom(prop => prop.Location.X)).ReverseMap();

        CreateMap<MovieCreationDto, Movie>()
            .ForMember(x => x.Poster, options => options.Ignore())
            .ForMember(x => x.MoviesGenres, options => options.MapFrom(MapMoviesGenres))
            .ForMember(x => x.MovieTheatersMovies, options => options.MapFrom(MapMovieTheatersMovies))
            .ForMember(x => x.MoviesActors, options => options.MapFrom(MapMoviesActors));

    }
    
        private List<ActorsMovieDto> MapMoviesActors(Movie movie, MovieDto movieDto)
        {
            var result = new List<ActorsMovieDto>();

            if (movie.MoviesActors == null) return result;
            result.AddRange(movie.MoviesActors.Select(moviesActors => new ActorsMovieDto()
            {
                Id = moviesActors.ActorId,
                Name = moviesActors.Actor.Name,
                Character = moviesActors.Character,
                Picture = moviesActors.Actor.Picture,
                Order = moviesActors.Order
            }));

            return result;
        }

        private List<MovieTheaterDto> MapMovieTheatersMovies(Movie movie, MovieDto movieDto)
        {
            var result = new List<MovieTheaterDto>();

            if (movie.MovieTheatersMovies == null) return result;
            result.AddRange(movie.MovieTheatersMovies.Select(
                movieTheaterMovies => new MovieTheaterDto()
                    { Id = movieTheaterMovies.MovieTheaterId, 
                        Name = movieTheaterMovies.MovieTheater.Name, 
                        Latitude = movieTheaterMovies.MovieTheater.Location.Y, 
                        Longitude = movieTheaterMovies.MovieTheater.Location.X
                        
                    }));

            return result;
        }

        private List<GenreDto> MapMoviesGenres(Movie movie, MovieDto movieDto)
        {
            var result = new List<GenreDto>();

            if (movie.MoviesGenres == null) return result;
            result.AddRange(movie.MoviesGenres.Select(genre => new GenreDto() { Id = genre.GenreId, Name = genre.Genre.Name }));

            return result;
        }

        private static List<MoviesGenres> MapMoviesGenres(MovieCreationDto movieCreationDto, Movie movie)
        {
            var result = new List<MoviesGenres>();

            if (movieCreationDto.GenresIds == null) { return result; }

            result.AddRange(movieCreationDto.GenresIds.Select(x => new MoviesGenres() { GenreId = x }));
           
            return result;
        }

        private static List<MovieTheatersMovies> MapMovieTheatersMovies(MovieCreationDto movieCreationDto,
            Movie movie)
        {
            var result = new List<MovieTheatersMovies>();

            if (movieCreationDto.MovieTheatersIds == null) { return result; }
          result.AddRange(movieCreationDto.MovieTheatersIds.Select(id => new MovieTheatersMovies() { MovieTheaterId = id }));

        return result;
        }

        private static List<MoviesActors> MapMoviesActors(MovieCreationDto movieCreationDto, Movie movie)
        {
            var result = new List<MoviesActors>();

            if (movieCreationDto.Actors == null) { return result; }

            result.AddRange(movieCreationDto.Actors.Select(actor => new MoviesActors() { ActorId = actor.Id, Character = actor.Character }));

            return result;
        }
    
}