using FlowMind.Core.DbContextOptions;
using FlowMind.Core.DTOs;
using FlowMind.Core.Interfaces;
using FlowMind.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Google.Apis.Auth;

namespace FlowMind.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepo;
        private readonly ITokenService _tokenService;

        public AuthController(IAuthRepository authRepo, ITokenService tokenService)
        {
            _authRepo = authRepo;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto registerDto)
        {
            if (await _authRepo.UserExists(registerDto.Email))
                return BadRequest(ApiResponse<string>.Failure("Registration failed",new List<string> { "Email is already registered." }));

            var userToCreate = new User
            {
                Email = registerDto.Email.ToLower(),
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PreferredCurrency = registerDto.PreferredCurrency,
            };

            var createdUser = await _authRepo.Register(userToCreate, registerDto.Password);

            var userResponse = new UserResponseDto
            {
                Id = Guid.Parse(createdUser.Id),
                Email = createdUser.Email,
                FirstName = createdUser.FirstName,
                LastName = createdUser.LastName,
                PreferredCurrency = createdUser.PreferredCurrency,
                Token = _tokenService.CreateToken(createdUser)
            };

            return Ok(ApiResponse<UserResponseDto>.Success(userResponse, "User registered successfully."));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
        {
            var user = await _authRepo.Login(loginDto.Email, loginDto.Password);

            if (user == null)
                return Unauthorized(ApiResponse<string>.Failure("Authentication failed", new List<string> { "Invalid email or password." }));

            var userResponse = new UserResponseDto
            {
                Id = Guid.Parse(user.Id),
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PreferredCurrency = user.PreferredCurrency,
                Token = _tokenService.CreateToken(user)
            };

            return Ok(ApiResponse<UserResponseDto>.Success(userResponse, "Login successful."));        }
    [HttpPost("google-login")]
public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto googleLoginDto)
{
    var settings = new GoogleJsonWebSignature.ValidationSettings
    {
        Audience = new[]
        {
            HttpContext.RequestServices
                .GetRequiredService<IConfiguration>()["GoogleAuth:ClientId"]
        }
    };

    GoogleJsonWebSignature.Payload payload;

    try
    {
        payload = await GoogleJsonWebSignature.ValidateAsync(
            googleLoginDto.IdToken,
            settings
        );
    }
    catch
    {
        return Unauthorized(
            ApiResponse<string>.Failure(
                "Google authentication failed",
                new List<string> { "Invalid Google token." }
            )
        );
    }

    var email = payload.Email.ToLower();

    var user = await _authRepo.GetUserByEmail(email);

    if (user == null)
    {
        user = new User
        {
            Email = email,
            FirstName = payload.GivenName ?? "",
            LastName = payload.FamilyName ?? "",
            PreferredCurrency = "ILS"
        };

        user = await _authRepo.RegisterGoogleUser(user);
    }

    var userResponse = new UserResponseDto
    {
        Id = Guid.Parse(user.Id),
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        PreferredCurrency = user.PreferredCurrency,
        Token = _tokenService.CreateToken(user)
    };

    return Ok(ApiResponse<UserResponseDto>.Success(userResponse, "Google login successful."));
}
}}