using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityComplaintApp.Models
{
    public class Complaint
    {
        [Key]
        public int ComplaintId { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public User? User { get; set; }

        [Required, StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required, StringLength(255)]
        public string Location { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Photo { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending | In Progress | Resolved

        [StringLength(50)]
        public string AssignedDept { get; set; } = "Unassigned";
    }
}
