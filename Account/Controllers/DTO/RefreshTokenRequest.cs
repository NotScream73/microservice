using System.ComponentModel.DataAnnotations;

namespace Account.Controllers.DTO
{
    public class RefreshTokenRequest
    {
        [Required]
        [MinLength(1, ErrorMessage = "Токен должен быть не меньше 1 символа в длину")]
        public string RefreshToken { get; set; }
    }
}
