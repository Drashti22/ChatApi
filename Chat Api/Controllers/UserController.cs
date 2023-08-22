using Chat_Api.Context;
using Chat_Api.Helpers;
using Chat_Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;

namespace Chat_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _authContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserController(AppDbContext appDbContext, IHttpContextAccessor httpContextAccessor)
        {
            _authContext = appDbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        //User Login

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] User userObj)
        {
            if(userObj == null)
            
                return BadRequest();
                var user = await _authContext.Users
                    .FirstOrDefaultAsync(
                    x=>x.Email == userObj.Email);
                if(user == null)
           
                    return NotFound(new { message = "login failed due to Invalid credentials !!" });

            if (!PasswordHasher.VerifyPassword(userObj.Password, user.Password))
            {
                return BadRequest(new
                {
                    Message = "Password is incorrect !!"
                });
            }

            user.Token = createJwtToken(user);

            var userResponse = new
            {
                userId = user.Id,
                name = user.Name,
                email = user.Email,
                Token = user.Token
            };
            return Ok(new
                {
                    Token= "",
                    Message = "Login Success!",
                    Profile = userResponse
                });
        }

        //User Registration

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] User userObj)
        {

            //Check email
            if (await _authContext.Users.AnyAsync(u => u.Email.ToLower() == userObj.Email.ToLower()))
                return Conflict(new { message = "Email already exists." });

            // Perform manual validation
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (userObj == null)
                return BadRequest();


            userObj.Password = PasswordHasher.HashPassword(userObj.Password);
            userObj.Token = "";
            await _authContext.Users.AddAsync(userObj);
            await _authContext.SaveChangesAsync();



            var userResponse = new
            {
                userId = userObj.Id,
                name = userObj.Name,
                email = userObj.Email
            };
            return Ok(new
                {
                Message = "User Registered !!",
                User = userResponse
            });
        }

        private string createJwtToken(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("This is my 128 bits very long secret key.......");
            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, $"{user.Name}"),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            });
            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.Now.AddDays(3),
                SigningCredentials = credentials,
            };
            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(token);
        }

        //Retrieve UserList

        private Models.User GetCurrentLoggedInUser()
        {
            var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                var currentUser = _authContext.Users.FirstOrDefault(u => u.Id == userId);
                return currentUser;
            }
            return null;
        }


        [Authorize]
        [HttpGet]
        public async Task<ActionResult<Models.User>> GetAllUsers()
        {
            var currentUser = GetCurrentLoggedInUser();
            if (currentUser == null)
            {
                return BadRequest(new { Message = "Unable to retrieve current user." });
            }
            var userList = await _authContext.Users
        .Where(u => u.Id != currentUser.Id)
        .Select(u => new
        {
            id = u.Id,
            name = u.Name,
            email = u.Email,
        })
        .ToListAsync();
            return Ok(new { users = userList });
        }





        //[HttpGet]
        //[Authorize]
        //public IActionResult GetAllUsers()
        //{
        //    string authenticatedUserEmail = User.Identity.Name;

        //    var users = _authContext.Users
        //        .Where(u => u.Email != authenticatedUserEmail)
        //        .Select(u => new
        //        {
        //            u.Id,
        //            u.Name,
        //            u.Email
        //        })
        //        .ToList();

        //    return Ok(new { users });
        //}





        //public async Task<ActionResult<User>> GetAllUsers()
        //{
        //    // Get the current user
        //    var authenticatedUserEmail = HttpContext.User;

        //    // Access user properties
        //    var userId = authenticatedUserEmail.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    var userName = authenticatedUserEmail.FindFirst(ClaimTypes.Name)?.Value;
        //    var userEmail = authenticatedUserEmail.FindFirst(ClaimTypes.Email)?.Value;
        //    await Console.Out.WriteLineAsync(userId);

        //    var users = await _authContext.Users
        //        .Where(u => u.Id != Convert.ToInt32(userId))
        //        .Select(u => new 
        //        {
        //            UserId = u.Id,
        //            Name = u.Name,
        //            Email = u.Email
        //        })
        //        .ToListAsync();

        //    return Ok(users);
        //}



    }
}
