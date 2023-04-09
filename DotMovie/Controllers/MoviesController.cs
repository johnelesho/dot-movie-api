using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DotMovie;
using DotMovie.Dtos;
using DotMovie.Entities;
using DotMovie.Helpers;

namespace DotMovie.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly AppDbContext context;
        private readonly IMapper mapper;
        private readonly ILogger<MoviesController> logger;
        private readonly IFileStorageService fileStorageService;
        private readonly FolderNames container;

        public MoviesController(AppDbContext context, IMapper mapper, ILogger<MoviesController> logger, IFileStorageService storageService)
        {
            this.context = context;
            this.mapper = mapper;
            this.logger = logger;
            fileStorageService = storageService;
        }

        [HttpGet]
        public async Task<ActionResult<HomeDto>> Get()
        { 
            const int top = 6;
            var today = DateTime.Today;

            var upcomingReleases = await context.Movies
                .Where(x => x.ReleaseDate > today)
                .OrderBy(x => x.ReleaseDate)
                .Take(top)
                .ToListAsync();

            var inTheaters = await context.Movies
                .Where(x => x.InTheaters)
                .OrderBy(x => x.ReleaseDate)
                .Take(top)
                .ToListAsync();

            var homeDto = new HomeDto
            {
                UpcomingReleases = mapper.Map<List<MovieDto>>(upcomingReleases),
                InTheaters = mapper.Map<List<MovieDto>>(inTheaters)
            };
            return homeDto;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MovieDto>> Get(int id)
        {
            var movie = await context.Movies
                .Include(x => x.MoviesGenres).ThenInclude(x => x.Genre)
                .Include(x => x.MovieTheatersMovies).ThenInclude(x => x.MovieTheater)
                .Include(x => x.MoviesActors).ThenInclude(x => x.Actor)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            var Dto = mapper.Map<MovieDto>(movie);
            Dto.Actors = Dto.Actors.OrderBy(x => x.Order).ToList();
            return Dto;
        }

        [HttpGet("filter")]
        public async Task<ActionResult<List<MovieDto>>> Filter([FromQuery] FilterMoviesDto filterMoviesDto)
        {
            var moviesQueryable = context.Movies.AsQueryable();

            if (!string.IsNullOrEmpty(filterMoviesDto.Title))
            {
                moviesQueryable = moviesQueryable.Where(x => x.Title.Contains(filterMoviesDto.Title));
            }

            if (filterMoviesDto.InTheaters)
            {
                moviesQueryable = moviesQueryable.Where(x => x.InTheaters);
            }

            if (filterMoviesDto.UpcomingReleases)
            {
                var today = DateTime.Today;
                moviesQueryable = moviesQueryable.Where(x => x.ReleaseDate > today);
            }

            if (filterMoviesDto.GenreId != 0)
            {
                moviesQueryable = moviesQueryable
                    .Where(x => x.MoviesGenres.Select(y => y.GenreId)
                    .Contains(filterMoviesDto.GenreId));
            }

            await HttpContext.InsertParametersPaginationInHeader(moviesQueryable);
            var movies = await moviesQueryable.OrderBy(x => x.Title).Paginate(filterMoviesDto.PaginationDto)
                .ToListAsync();
            return mapper.Map<List<MovieDto>>(movies);
        }

        [HttpGet("PostGet")]
        public async Task<ActionResult<MoviePostGetDto>> PostGet()
        {
            var movieTheaters = await context.MovieTheaters.OrderBy(x => x.Name).ToListAsync();
            var genres = await context.Genres.OrderBy(x => x.Name).ToListAsync();

            var movieTheatersDto = mapper.Map<List<MovieTheaterDto>>(movieTheaters);
            var genresDto = mapper.Map<List<GenreDto>>(genres);

            return new MoviePostGetDto() { Genres = genresDto, MovieTheaters = movieTheatersDto };
        }

        [HttpPost]
        public async Task<ActionResult<int>> Post([FromForm] MovieCreationDto movieCreationDto)
        {
            var movie = mapper.Map<Movie>(movieCreationDto);

            if (movieCreationDto.Poster != null)
            {
                movie.Poster = await fileStorageService.SaveFile(container, movieCreationDto.Poster);
            }

            AnnotateActorsOrder(movie);
            context.Add(movie);
            await context.SaveChangesAsync();
            return movie.Id;
        }

        [HttpGet("putget/{id:int}")]
        public async Task<ActionResult<MoviePutGetDto>> PutGet(int id)
        {
            var movieActionResult = await Get(id);
            if (movieActionResult.Result is NotFoundResult) { return NotFound(); }

            var movie = movieActionResult.Value;

            var genresSelectedIds = movie.Genres.Select(x => x.Id).ToList();
            var nonSelectedGenres = await context.Genres.Where(x => !genresSelectedIds.Contains(x.Id))
                .ToListAsync();

            var movieTheatersIds = movie.MovieTheaters.Select(x => x.Id).ToList();
            var nonSelectedMovieTheaters = await context.MovieTheaters.Where(x =>
            !movieTheatersIds.Contains(x.Id)).ToListAsync();

            var nonSelectedGenresDtos = mapper.Map<List<GenreDto>>(nonSelectedGenres);
            var nonSelectedMovieTheatersDto = mapper.Map<List<MovieTheaterDto>>(nonSelectedMovieTheaters);

            var response = new MoviePutGetDto();
            response.Movie = movie;
            response.SelectedGenres = movie.Genres;
            response.NonSelectedGenres = nonSelectedGenresDtos;
            response.SelectedMovieTheaters = movie.MovieTheaters;
            response.NonSelectedMovieTheaters = nonSelectedMovieTheatersDto;
            response.Actors = movie.Actors;
            return response;
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromForm] MovieCreationDto movieCreationDto)
        {
            var movie = await context.Movies.Include(x => x.MoviesActors)
                .Include(x => x.MoviesGenres)
                .Include(x => x.MovieTheatersMovies)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            movie = mapper.Map(movieCreationDto, movie);
             
            if (movieCreationDto.Poster != null)
            {
                movie.Poster = await fileStorageService.EditFile(container, movieCreationDto.Poster,
                    movie.Poster);
            }

            AnnotateActorsOrder(movie);
            await context.SaveChangesAsync();
            return NoContent();
        }

        private static void AnnotateActorsOrder(Movie movie)
        {
            if (movie.MoviesActors == null) return;
            for (var i = 0; i < movie.MoviesActors.Count; i++)
            {
                movie.MoviesActors[i].Order = i;
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var movie = await context.Movies.FirstOrDefaultAsync(x => x.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            context.Remove(movie);
            await context.SaveChangesAsync();
            await fileStorageService.DeleteFile(movie.Poster, container);
            return NoContent();
        }
    }
}
