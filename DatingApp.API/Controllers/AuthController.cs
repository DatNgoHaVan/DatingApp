using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
  // http://localhost:5000/api/
  // controller in api/controller will 
  //be replaced with 1st part of `Auth` in `AuthController`
  [Route("api/[controller]")]
  [ApiController]
  public class AuthController : ControllerBase
  {
    private readonly IAuthRepository _repo;
    private readonly IConfiguration _config;

    public AuthController(IAuthRepository repo, IConfiguration config)
    {
      _config = config;
      _repo = repo;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
    {

      userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

      if (await _repo.UserExists(userForRegisterDto.Username))
        return BadRequest("Username already exist");

      var userToCreate = new User
      {
        Username = userForRegisterDto.Username
      };

      // this register require user obj
      var createdUser = await _repo.Register(userToCreate, userForRegisterDto.Password);

      //status code create successful
      return StatusCode(201);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
    {
      // get user from repo
      var userFromRepo = await _repo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);

      //check userFromRepo
      if (userFromRepo == null)
        return Unauthorized();

      //create claims for JWT
      var claims = new[]
      {
        new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
        new Claim(ClaimTypes.Name, userFromRepo.Username)
      };

      //create key for tokens get section
      var key = new SymmetricSecurityKey(Encoding.UTF8
        .GetBytes(_config.GetSection("AppSettings:Token").Value));

      //create credential key
      var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

      // token desciptor for contains above claim and creds
      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.Now.AddDays(1),
        SigningCredentials = creds
      };

      var tokenHandler = new JwtSecurityTokenHandler();

      // create token
      var token = tokenHandler.CreateToken(tokenDescriptor);

      // return ok with a object token
      return Ok(new {
        token = tokenHandler.WriteToken(token)
      });
    }
  }
}