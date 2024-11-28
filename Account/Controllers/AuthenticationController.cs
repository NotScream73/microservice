using Account.Controllers.DTO;
using Account.Exceptions;
using Account.Services;
using Account.Services.DTO;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Account.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;

        public AuthenticationController(IMapper mapper, IUserService userService, ITokenService tokenService)
        {
            _mapper = mapper;
            _userService = userService;
            _tokenService = tokenService;
        }

        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userDTO = _mapper.Map<UserCreateDTO>(request);

            var userId = await _userService.CreateAsync(userDTO);

            var tokens = await _tokenService.GenerateTokensAsync(userId);

            return Ok(tokens);
        }

        [HttpPost("SignIn")]
        public async Task<IActionResult> SignIn([FromBody] SignInRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = await _userService.AuthenticateAsync(request.Username, request.Password);

            var tokens = await _tokenService.GenerateTokensAsync(userId);

            return Ok(_mapper.Map<SignInResponse>(tokens));
        }

        [HttpPut("SignOut")]
        [Authorize]
        public async Task<IActionResult> UserSignOut()
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                throw new ApiException("Неправильный токен.");
            }

            await _userService.LogoutAsync(userId);

            return Ok();
        }

        [HttpGet("Validate")]
        public async Task<IActionResult> Validate([FromQuery][Required] string accessToken)
        {
            var validationResult = await _tokenService.ValidateAccessToken(accessToken);
            return Ok(_mapper.Map<ValidationTokenResult>(validationResult));
        }

        [HttpPost("Refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tokens = await _tokenService.RefreshTokensAsync(request.RefreshToken);

            return Ok(_mapper.Map<RefreshTokenResponse>(tokens));
        }
    }
}
