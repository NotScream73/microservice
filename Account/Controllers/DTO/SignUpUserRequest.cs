using System.ComponentModel.DataAnnotations;

namespace Account.Controllers.DTO
{
    public class SignUpUserRequest
    {
        [Required]
        [MaxLength(256, ErrorMessage = "Фамилия должна быть не больше 256 символов в длину")]
        public string LastName { get; set; }
        [Required]
        [MaxLength(256, ErrorMessage = "Имя должно быть не больше 256 символов в длину")]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(256, ErrorMessage = "Имя пользователя должно быть не больше 256 символов в длину")]
        public string Username { get; set; }
        [Required]
        [MaxLength(256, ErrorMessage = "Пароль должен быть не больше 256 символов в длину")]
        public string Password { get; set; }
        [Required, MinLength(1)]
        public string[] Roles { get; set; }
    }
}
