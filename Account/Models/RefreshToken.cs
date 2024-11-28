using System.ComponentModel.DataAnnotations.Schema;

namespace Account.Models
{
    public class RefreshToken
    {
        [Column("user_id")]
        public int UserId { get; set; }
        [Column("token")]
        public string Token { get; set; }
        [Column("expires")]
        public DateTime Expires { get; set; }
        public virtual User User { get; set; }
    }
}
