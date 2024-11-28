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
    public class AccountsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public AccountsController(IMapper mapper, IUserService userService)
        {
            _mapper = mapper;
            _userService = userService;
        }

        [HttpGet("Me")]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                throw new ApiException("Информация о пользователе отсутствует.");
            }

            var userInfo = await _userService.GetMeInfoByIdAsync(userId);

            return Ok(_mapper.Map<AccountMeInfoResponse>(userInfo));
        }

        [HttpPut("Update")]
        [Authorize]
        public async Task<IActionResult> Update([FromBody] AccountUpdateRequest userUpdateDTO)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                throw new ApiException("Информация о пользователе отсутствует.");
            }

            var updateDTO = _mapper.Map<UserUpdateDTO>(userUpdateDTO);

            await _userService.UpdateAsync(userId, updateDTO);

            return Ok();
        }

        [HttpGet]
        [Authorize("Admin")]
        public async Task<IActionResult> GetAll([FromQuery][Required] int from, [FromQuery][Required] int count)
        {
            var list = await _userService.GetAllAsync(from, count);

            return Ok(list);
        }

        [HttpGet("{id:int}/Roles")]
        [Authorize("Admin")]
        public async Task<IActionResult> GetUserRoles([Required] int id)
        {
            var roles = await _userService.GetUserRolesByIdAsync(id);

            return Ok(roles);
        }

        [HttpPost("SignUp")]
        [Authorize("Admin")]
        public async Task<IActionResult> CreateUser([FromBody] SignUpUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userDTO = _mapper.Map<UserCreateByAdminDTO>(request);

            var userId = await _userService.CreateByAdminAsync(userDTO);

            return Ok(userId);
        }

        [HttpPut("{id:int}")]
        [Authorize("Admin")]
        public async Task<IActionResult> UpdateUser([Required] int id, [FromBody] UpdateUserRequest request)
        {
            var userDTO = _mapper.Map<UserUpdateByAdminDTO>(request);

            await _userService.UpdateUserAsync(id, userDTO);

            return Ok();
        }

        [HttpDelete("{id:int}")]
        [Authorize("Admin")]
        public async Task<IActionResult> DeleteUser([Required] int id)
        {
            await _userService.DeleteAsync(id);
            return Ok();
        }
    }
}