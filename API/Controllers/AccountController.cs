using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.DTOs;
using API.Services;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[AllowAnonymous, ApiController, Route("api/[controller]")]
public class AccountController(
	UserManager<AppUser> userManager,
	SignInManager<AppUser> signInManager,
	TokenService tokenService) : ControllerBase
{
	[HttpPost("login")]
	public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
	{
		var user = await userManager.Users.Include(p => p.Photos)
			.FirstOrDefaultAsync(x => x.Email == loginDto.Email);

		if (user == null) return Unauthorized();

		var result = await signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

		if (result.Succeeded) return CreateUserObject(user);

		return Unauthorized();
	}

	[HttpPost("register")]
	public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
	{
		if (await userManager.Users.AnyAsync(x => x.Email == registerDto.Email))
		{
			ModelState.AddModelError("email", "Email taken");
			return ValidationProblem();
		}

		if (await userManager.Users.AnyAsync(x => x.UserName == registerDto.Username))
		{
			ModelState.AddModelError("username", "Username taken");
			return ValidationProblem();
		}

		var user = new AppUser
		{
			DisplayName = registerDto.DisplayName,
			Email = registerDto.Email,
			UserName = registerDto.Username
		};

		var result = await userManager.CreateAsync(user, registerDto.Password);

		if (result.Succeeded) return CreateUserObject(user);

		return BadRequest("Problem registering user");
	}

	[Authorize]
	[HttpGet]
	public async Task<ActionResult<UserDto>> GetCurrentUser()
	{
		var user = await userManager.Users.Include(p => p.Photos)
			.FirstOrDefaultAsync(x => x.Email == User.FindFirstValue(ClaimTypes.Email));

		return CreateUserObject(user);
	}

	private UserDto CreateUserObject(AppUser user)
	{
		return new UserDto
		{
			DisplayName = user.DisplayName,
			Image = user?.Photos?.FirstOrDefault(x => x.IsMain)?.Url,
			Token = tokenService.CreateToken(user),
			Username = user.UserName
		};
	}
}