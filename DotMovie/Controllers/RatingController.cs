using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotMovie.Dtos;
using DotMovie.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotMovie.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatingController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public RatingController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Post([FromBody] RatingDto ratingDto)
        {
            var email = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "email").Value;
            var user = await _userManager.FindByEmailAsync(email);
            var userId = user.Id;

            var currentRate = await _context.Ratings.FirstOrDefaultAsync(
                x => x.MovieId == ratingDto.MovieId && x.UserId.ToString() == userId
            );

            if (currentRate != null)
            {
                currentRate.Rate = ratingDto.Rate;
            }
            else
            {
                var rating = new Rating()
                {
                    UserId = Convert.ToInt32(userId),
                    Rate = ratingDto.Rate,
                    MovieId = ratingDto.MovieId
                };
                _context.Add(rating);
                
            }
            _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
