using System.ComponentModel.DataAnnotations;

namespace CommunityComplaintApp.Models.ViewModels
{
    public class AdminLoginViewModel
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
