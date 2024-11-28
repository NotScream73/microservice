using System.ComponentModel.DataAnnotations.Schema;

namespace Account.Models
{
    public class UserRole
    {
        [Column("user_id")]
        public int UserId { get; set; }
        [Column("role_id")]
        public int RoleId { get; set; }
        public virtual Role Role { get; set; }
    }
}
