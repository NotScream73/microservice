using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Account.Data;
using Account.Exceptions;
using Account.Models;
using Account.Services.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Account.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly DataContext _context;

        public TokenService(IConfiguration configuration, DataContext context, IUserService userService)
        {
            _configuration = configuration;
            _context = context;
            _userService = userService;
        }

        public async Task<string> GenerateAccessTokenAsync(int userId)
        {
            if (!await _userService.DoesUserExistAsync(userId))
            {
                throw new ApiException($"Пользователь с ID {userId} не найден.");
            };

            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature);

            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new ClaimsIdentity();

            claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));

            var roleNames = await
                (
                    from ur in _context.UserRoles
                    join r in _context.Roles on ur.RoleId equals r.Id
                    where ur.UserId == userId
                    select new Claim(ClaimTypes.Role, r.Name)
                ).ToListAsync();

            claims.AddClaims(roleNames);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiresInMinutes"])),
                SigningCredentials = credentials
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<string> GenerateRefreshTokenAsync(int userId)
        {
            if (!await _userService.DoesUserExistAsync(userId))
            {
                throw new ApiException($"Пользователь с ID {userId} не найден.");
            }

            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.UserId == userId)
                ?? new RefreshToken
                {
                    UserId = userId
                };

            refreshToken.Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            refreshToken.Expires = DateTime.UtcNow.AddDays(double.Parse(_configuration["Jwt:RefreshTokenExpiresInDays"]));

            if (_context.Entry(refreshToken).State == EntityState.Detached)
            {
                await _context.RefreshTokens.AddAsync(refreshToken);
            }

            await _context.SaveChangesAsync();

            return refreshToken.Token;
        }

        public async Task<AccessAndRefreshTokenDTO> GenerateTokensAsync(int userId)
        {
            if (!await _userService.DoesUserExistAsync(userId))
            {
                throw new ApiException($"Пользователь с ID {userId} не найден.");
            }

            var accessToken = await GenerateAccessTokenAsync(userId);

            var refreshToken = await GenerateRefreshTokenAsync(userId);

            return new AccessAndRefreshTokenDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<TokenValidationResultDTO> ValidateAccessToken(string token)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true
            };

            try
            {
                var IsValid = true;

                var claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                var userId = int.Parse(claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier).Value);

                var roles = await
                    (
                        from u in _context.Users
                        join ur in _context.UserRoles on u.Id equals ur.UserId
                        join r in _context.Roles on ur.RoleId equals r.Id
                        where u.Id == userId && !u.IsDeleted && u.IsLogin
                        select r.Name
                    ).ToArrayAsync();

                var claimRoles = claimsPrincipal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();

                if (!Enumerable.SequenceEqual(claimRoles, roles))
                {
                    IsValid = false;
                }

                return new TokenValidationResultDTO
                {
                    IsValid = IsValid,
                    UserId = IsValid ? userId : null,
                    Roles = IsValid ? claimRoles : null
                };
            }
            catch (Exception)
            {
                return new TokenValidationResultDTO
                {
                    IsValid = false
                };
            }
        }

        public async Task<AccessAndRefreshTokenDTO> RefreshTokensAsync(string refreshTokenString)
        {
            var refreshToken = await
                (
                    from r in _context.RefreshTokens
                    join u in _context.Users on r.UserId equals u.Id
                    where r.Token == refreshTokenString && !u.IsDeleted && u.IsLogin
                    select r
                ).FirstOrDefaultAsync()
                ?? throw new ApiException("Токен невалидный.");

            var tokens = await GenerateTokensAsync(refreshToken.UserId);

            return tokens;
        }
    }
}
