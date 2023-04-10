using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DotMovie.Dtos;
using DotMovie.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;

namespace DotMovie.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<AccountsController> _logger;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public AccountsController(UserManager<IdentityUser> userManager, ILogger<AccountsController> logger,
            SignInManager<IdentityUser> signInManager, IMapper mapper, IConfiguration configuration, AppDbContext context)
        {
            _userManager = userManager;
            _logger = logger;
            _signInManager = signInManager;
            _mapper = mapper;
            _configuration = configuration;
            _context = context;
        }
        // GET: api/Accounts
        [HttpGet]
        public async Task<ActionResult<AuthenticationResponse>> Login([FromBody] UserCreate userCreate)
        {
            var result = await _signInManager.PasswordSignInAsync(
                userCreate.Email, userCreate.Password, false, false);
            if (result.Succeeded)
            {
                return await BuildToken(userCreate);
            }
            else
            {
                return BadRequest("Login Failed!!");
            }
         
        }

        [HttpGet("list-all-users")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "IsAdmin")]
        public async Task<ActionResult<List<UserDto>>> getAllUsers([FromQuery] PaginationDto paginationDto)
        {
            var allUsers = _context.Users.AsQueryable();
            await HttpContext.InsertParametersPaginationInHeader(allUsers);

           var users = allUsers.OrderBy(x => x.Email).Paginate(paginationDto).ToListAsync();
           return _mapper.Map<List<UserDto>>(users);
        }
        
        [HttpGet("make-admin")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "IsAdmin")]
        public async Task<ActionResult<List<UserDto>>> MakeAdmin([FromBody] string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null) return NotFound();

            await _userManager.AddClaimAsync(user, new Claim("role", "admin"));

            return NoContent();
        }
        
        [HttpGet("remove-admin")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "IsAdmin")]
        public async Task<ActionResult<List<UserDto>>> RemoveAdmin([FromBody] string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null) return NotFound();

            await _userManager.RemoveClaimAsync(user, new Claim("role", "admin"));

            return NoContent();
        }




        // POST: api/Accounts
        [HttpPost]
        public async Task<ActionResult<AuthenticationResponse>> Register([FromBody] UserCreate request)
        {
            var user = new IdentityUser
            {
                UserName = request.Email,
                Email = request.Email

            };
            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                return await BuildToken(request);
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }
        

        private async Task<AuthenticationResponse> BuildToken(UserCreate user)
        {
            var claims = new List<Claim>()
            {
                new Claim("email", user.Email)
             
            };
            var userFound = await _userManager.FindByEmailAsync(user.Email);
            var claimDb = await _userManager.GetClaimsAsync(userFound);
          
            claims.AddRange(claimDb);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["jwt_key"]));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.Now.AddHours(2);
            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: expiration,
                signingCredentials: cred
            );

            return new AuthenticationResponse()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                expirationTime = expiration
            };
        }
    }
}
