using CommunityComplaintApp.Data;
using CommunityComplaintApp.Models;
using CommunityComplaintApp.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommunityComplaintApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<Admin> _hasher = new();
        private const string SessionAdminId = "AdminId";
        private const string SessionAdminUsername = "AdminUsername";

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsLoggedIn() => HttpContext.Session.GetInt32(SessionAdminId) != null;

        // GET: /Admin/Login
        [HttpGet]
        public IActionResult Login()
        {
            if (IsLoggedIn())
            {
                return RedirectToAction(nameof(Dashboard));
            }
            return View(new AdminLoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AdminLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var admin = await _context.Admins
                .FirstOrDefaultAsync(a => a.Username == model.Username);

            if (admin == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            var result = _hasher.VerifyHashedPassword(admin, admin.Password, model.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            HttpContext.Session.SetInt32(SessionAdminId, admin.AdminId);
            HttpContext.Session.SetString(SessionAdminUsername, admin.Username);

            return RedirectToAction(nameof(Dashboard));
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }

        // GET: /Admin/Dashboard
        // Tabular view of all complaints with status filtering and search (per Website Modules spec).
        public async Task<IActionResult> Dashboard(string? status, string? search)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction(nameof(Login));
            }

            var query = _context.Complaints.Include(c => c.User).AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(c => c.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    c.ComplaintId.ToString() == search ||
                    (c.User != null && c.User.MobileNumber.Contains(search)) ||
                    c.Category.Contains(search) ||
                    c.Location.Contains(search));
            }

            var complaints = await query.OrderByDescending(c => c.Date).ToListAsync();

            ViewBag.CurrentStatus = status ?? "All";
            ViewBag.CurrentSearch = search ?? string.Empty;

            return View(complaints);
        }

        // POST: /Admin/UpdateComplaint
        // Assign department and/or change status for a single ticket.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateComplaint(int complaintId, string status, string assignedDept)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction(nameof(Login));
            }

            var complaint = await _context.Complaints.FindAsync(complaintId);
            if (complaint == null)
            {
                return NotFound();
            }

            complaint.Status = status;
            complaint.AssignedDept = assignedDept;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Dashboard));
        }
    }
}
