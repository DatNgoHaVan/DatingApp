using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
  // http://localhost:5000/api/
  // controller in api/controller will be replaced with 1st part of `Auth` in `AuthController`
  [Route("api/[controller]")]
  [ApiController]
  public class AuthController : ControllerBase
  {
    private readonly IAuthRepository _repo;

    public AuthController(IAuthRepository repo)
    {
      _repo = repo;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
    {
      //validate request

      userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

      if(await _repo.UserExists(userForRegisterDto.Username))
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
  }
}