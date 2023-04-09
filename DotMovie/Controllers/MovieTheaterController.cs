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

namespace DotMovie.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovieTheaterController : ControllerBase
    {
        private IMapper _mapper;
        private readonly AppDbContext _context;
        private readonly ILogger<MovieTheater> _logger;

        public MovieTheaterController(AppDbContext context, ILogger<MovieTheater> logger, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
            _logger = logger;
        }

        // GET: api/MovieTheater
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MovieTheaterDto>>> GetMovieTheaters()
        {
          if (_context.MovieTheaters == null)
          {
              return NotFound();
          }

          var movieTheaters = await _context.MovieTheaters.ToListAsync();
          return _mapper.Map<List<MovieTheaterDto>>(movieTheaters);
        }

        // GET: api/MovieTheater/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MovieTheaterDto>> GetMovieTheater(int id)
        {
          if (_context.MovieTheaters == null)
          {
              return NotFound();
          }
            var movieTheater = await _context.MovieTheaters.FindAsync(id);

            if (movieTheater == null)
            {
                return NotFound();
            }

            return _mapper.Map<MovieTheaterDto>(movieTheater);
        }

        // PUT: api/MovieTheater/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMovieTheater(int id, MovieTheaterCreationDto movieTheater)
        {
            var mv = await _context.MovieTheaters.FindAsync(id);

            if (mv == null)
            {
                return NotFound();
            }

            mv = _mapper.Map<MovieTheater>(movieTheater);
            _context.MovieTheaters.Add(mv);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MovieTheaterExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/MovieTheater
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<MovieTheater>> PostMovieTheater(MovieTheaterDto movieTheater)
        {
          if (_context.MovieTheaters == null)
          {
              return Problem("Entity set 'AppDbContext.MovieTheaters'  is null.");
          }

          var mv = _mapper.Map<MovieTheater>(movieTheater);
            _context.MovieTheaters.Add(mv);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMovieTheater", new { id = movieTheater.Id }, movieTheater);
        }

        // DELETE: api/MovieTheater/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovieTheater(int id)
        {
            if (_context.MovieTheaters == null)
            {
                return NotFound();
            }
            var movieTheater = await _context.MovieTheaters.FindAsync(id);
            if (movieTheater == null)
            {
                return NotFound();
            }

            _context.MovieTheaters.Remove(movieTheater);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MovieTheaterExists(int id)
        {
            return (_context.MovieTheaters?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
