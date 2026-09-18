using CommunityComplaintApp.Data;
using CommunityComplaintApp.Models;
using CommunityComplaintApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommunityComplaintApp.Controllers
{
    public class ComplaintController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        private const string SessionUserId = "CitizenUserId";
        private const string SessionUserName = "CitizenName";

        public ComplaintController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: /Complaint/Register
        // Step 1 of the citizen flow — collects Name + Mobile Number only (low-friction registration).
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Reuse the existing user record if this mobile number has registered before.
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.MobileNumber == model.MobileNumber);

            if (user == null)
            {
                user = new User
                {
                    Name = model.Name,
                    MobileNumber = model.MobileNumber
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            HttpContext.Session.SetInt32(SessionUserId, user.UserId);
            HttpContext.Session.SetString(SessionUserName, user.Name);

            return RedirectToAction(nameof(Submit));
        }

        // GET: /Complaint/Submit
        // Step 2 — the complaint form (Category, Description, Location, optional Photo).
        [HttpGet]
        public IActionResult Submit()
        {
            var userId = HttpContext.Session.GetInt32(SessionUserId);
            if (userId == null)
            {
                return RedirectToAction(nameof(Register));
            }

            ViewBag.Categories = new List<string>
            {
                "Garbage", "Water", "Streetlight", "Pothole", "Drainage", "Other"
            };

            return View(new ComplaintViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(ComplaintViewModel model)
        {
            var userId = HttpContext.Session.GetInt32(SessionUserId);
            if (userId == null)
            {
                return RedirectToAction(nameof(Register));
            }

            ViewBag.Categories = new List<string>
            {
                "Garbage", "Water", "Streetlight", "Pothole", "Drainage", "Other"
            };

            if (model.Category == "Other" && string.IsNullOrWhiteSpace(model.OtherCategoryText))
            {
                ModelState.AddModelError("OtherCategoryText", "Please specify the issue category.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // When "Other" is chosen, store the citizen's own category text instead of the literal word "Other".
            var finalCategory = model.Category == "Other"
                ? model.OtherCategoryText!.Trim()
                : model.Category;

            string? photoPath = null;

            if (model.Photo != null && model.Photo.Length > 0)
            {
                // Basic file-type and size validation (matches TC_U05 / TC_U06 in the test plan).
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(model.Photo.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("Photo", "Only JPG and PNG image files are allowed.");
                    return View(model);
                }

                if (model.Photo.Length > 2 * 1024 * 1024) // 2 MB
                {
                    ModelState.AddModelError("Photo", "Photo must be smaller than 2 MB.");
                    return View(model);
                }

                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Photo.CopyToAsync(stream);
                }

                photoPath = $"/uploads/{uniqueFileName}";
            }

            var complaint = new Complaint
            {
                UserId = userId.Value,
                Category = finalCategory,
                Description = model.Description,
                Location = model.Location,
                Photo = photoPath,
                Date = DateTime.Now,
                Status = "Pending",
                AssignedDept = "Unassigned"
            };

            _context.Complaints.Add(complaint);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Confirmation), new { id = complaint.ComplaintId });
        }

        // GET: /Complaint/Confirmation/5
        public async Task<IActionResult> Confirmation(int id)
        {
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint == null)
            {
                return NotFound();
            }

            return View(complaint);
        }

        // GET: /Complaint/Track
        // Publicly accessible tracking page (Complaint ID + Mobile Number).
        [HttpGet]
        public IActionResult Track()
        {
            return View(new TrackViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Track(TrackViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var complaint = await _context.Complaints
                .Include(c => c.User)
                .FirstOrDefaultAsync(c =>
                    c.ComplaintId == model.ComplaintId &&
                    c.User != null &&
                    c.User.MobileNumber == model.MobileNumber);

            if (complaint == null)
            {
                ModelState.AddModelError(string.Empty, "No records found for the given Complaint ID and Mobile Number.");
                return View(model);
            }

            return View("TrackResult", complaint);
        }
    }
}
