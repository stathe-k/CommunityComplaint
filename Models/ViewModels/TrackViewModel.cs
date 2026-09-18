using System.ComponentModel.DataAnnotations;

namespace CommunityComplaintApp.Models.ViewModels
{
    public class TrackViewModel
    {
        [Required(ErrorMessage = "Complaint ID is required")]
        [Display(Name = "Complaint ID")]
        public int ComplaintId { get; set; }

        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10-digit mobile number")]
        [Display(Name = "Mobile Number")]
        public string MobileNumber { get; set; } = string.Empty;
    }
}
