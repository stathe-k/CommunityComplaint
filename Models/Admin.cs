using System.ComponentModel.DataAnnotations;

namespace CommunityComplaintApp.Models
{
    public class Admin
    {
        [Key]
        public int AdminId { get; set; }

        [Required, StringLength(50)]
        public string Username { get; set; } = string.Empty;

        // Stores a salted hash, never plain text (see PasswordHasher usage in AdminController)
        [Required, StringLength(255)]
        public string Password { get; set; } = string.Empty;
    }
}
