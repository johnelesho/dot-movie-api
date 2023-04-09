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
    public class ActorsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ActorsController> _logger;
        private readonly IFileStorageService _fileStorageService;
        private readonly  FolderNames containerName =  FolderNames.actors;


        public ActorsController(AppDbContext context, IMapper mapper, ILogger<ActorsController> logger, IFileStorageService fileStorageService)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _fileStorageService = fileStorageService;
        }

        // GET: api/Actors
        // [HttpGet]
        // public async Task<ActionResult<IEnumerable<ActorDto>>> GetActors()
        // {
        //     var listAsync = await _context.Actors.OrderBy(x=>x.Name).ToListAsync();
        //   return _mapper.Map<List<ActorDto>>(listAsync);
        // }
        
        [HttpGet]
        public async Task<ActionResult<List<ActorDto>>> Get([FromQuery] PaginationDto paginationDTO)
        {
            var queryable = _context.Actors.AsQueryable();
            await HttpContext.InsertParametersPaginationInHeader(queryable);
            var actors = await queryable.OrderBy(x => x.Name).Paginate(paginationDTO).ToListAsync();
            return _mapper.Map<List<ActorDto>>(actors);
        }

        [HttpPost("searchByName")]
        public async Task<ActionResult<List<ActorsMovieDto>>> SearchByName([FromBody] string name)
        {
            if (string.IsNullOrWhiteSpace(name)) { return new List<ActorsMovieDto>(); }
            return await _context.Actors
                .Where(x => x.Name.Contains(name))
                .OrderBy(x => x.Name)
                .Select(x => new ActorsMovieDto { Id = x.Id, Name = x.Name, Picture = x.Picture })
                .Take(5)
                .ToListAsync();
        }
        // GET: api/Actors/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ActorDto>> GetActor(int id)
        {
            var actor = await _context.Actors.FirstOrDefaultAsync(x=> x.Id == id);

            if (actor == null)
            {
                return NotFound();
            }

            return _mapper.Map<ActorDto>(actor);
        }

        // PUT: api/Actors/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutActor(int id, [FromForm] ActorCreationDto actorRequest)
        {
            var actor = await _context.Actors.FirstOrDefaultAsync(x => x.Id == id);

            if (actor == null)
            {
                return NotFound();
            }

            actor = _mapper.Map(actorRequest, actor);

            if (actorRequest.Picture != null)
            {
                actor.Picture = await _fileStorageService.EditFile(containerName,
                    actorRequest.Picture, actor.Picture);
            }

            await _context.SaveChangesAsync();
            return NoContent();
            
        }

        // POST: api/Actors
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Actor>> PostActor([FromForm] ActorCreationDto request)
        {
            var actor = _mapper.Map<Actor>(request);

            if (request.Picture != null)
            {
                
                actor.Picture = await _fileStorageService.SaveFile(containerName, request.Picture);
            }
            _context.Actors.Add(actor);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetActor", new { id = actor.Id }, actor);
        }

        // DELETE: api/Actors/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteActor(int id)
        {
            var actor = await _context.Actors.FirstOrDefaultAsync(x=> x.Id == id);
            if (actor == null)
            {
                return NotFound();
            }
            
            
            await _fileStorageService.DeleteFile(actor.Picture, containerName);

            _context.Actors.Remove(actor);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ActorExists(int id)
        {
            return (_context.Actors?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
