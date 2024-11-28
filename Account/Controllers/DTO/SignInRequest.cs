using System.ComponentModel.DataAnnotations;

namespace Account.Controllers.DTO
{
    public class SignInRequest
    {
        [Required]
        [StringLength(256, ErrorMessage = "Имя пользователя должно быть не больше 256 символов в длину")]
        public string Username { get; set; }
        [Required]
        [StringLength(256, ErrorMessage = "Пароль должен быть не больше 256 символов в длину")]
        public string Password { get; set; }
    }
}
