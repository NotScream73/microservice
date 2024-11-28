using System.ComponentModel.DataAnnotations.Schema;

namespace Account.Models
{
    public class User
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("last_name")]
        public string LastName { get; set; }
        [Column("first_name")]
        public string FirstName { get; set; }
        [Column("user_name")]
        public string UserName { get; set; }
        [Column("password_hash")]
        public string PasswordHash { get; set; }
        [Column("is_deleted")]
        public bool IsDeleted { get; set; }
        [Column("is_login")]
        public bool IsLogin { get; set; }
        public virtual ICollection<UserRole>? UserRoles { get; set; }
    }
}
