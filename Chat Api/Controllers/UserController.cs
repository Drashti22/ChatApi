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

namespace Chat_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _authContext;
        public UserController(AppDbContext appDbContext)
        {
            _authContext = appDbContext;
        }


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
                new Claim(ClaimTypes.Name, $"{user.Name}")
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



        [HttpGet]
        public async Task <ActionResult<User>> GetAllUsers()
        {
            return Ok(await _authContext.Users.ToListAsync());
        }


    }
}
