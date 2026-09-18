using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CommunityComplaintApp.Models.ViewModels
{
    public class ComplaintViewModel
    {
        [Required(ErrorMessage = "Please select a category")]
        [Display(Name = "Category")]
        public string Category { get; set; } = string.Empty;

        // Only used when Category == "Other" — the citizen's free-text category name.
        [Display(Name = "Please specify")]
        public string? OtherCategoryText { get; set; }

        [Required(ErrorMessage = "Please describe the issue")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the location")]
        [Display(Name = "Location")]
        public string Location { get; set; } = string.Empty;

        [Display(Name = "Photo (optional)")]
        public IFormFile? Photo { get; set; }
    }
}
