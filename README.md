# Community Complaint Reporting Website — ASP.NET Core

ASP.NET Core 8.0 MVC implementation of the project described in the
Community Engagement Project report (User/Complaint/Admin schema,
citizen registration by Name + Mobile Number, complaint submission
with photo upload, public tracking, and an admin dashboard).

**Visual theme:** dark background, green accent, translucent glass
panels, and pill-shaped buttons — matching the reference dashboard
design concept the client provided. All styling lives in
`wwwroot/css/site.css`; Bootstrap is still used for grid/layout, with
its default colors overridden.

## Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB, Express, or full SQL Server)

## Setup

1. **Configure the connection string** in `appsettings.json`
   (`ConnectionStrings:DefaultConnection`). The default targets a
   local `SQLEXPRESS` instance — change it to match your setup, e.g.
   for LocalDB:
   ```
   Server=(localdb)\\mssqllocaldb;Database=CommunityComplaintDB;Trusted_Connection=True;
   ```

2. **Create the database.** Two options:

   - **Option A — EF Core Migrations (recommended):**
     ```bash
     dotnet tool install --global dotnet-ef   # if not already installed
     dotnet ef migrations add InitialCreate
     dotnet ef database update
     ```
     The app also calls `Database.Migrate()` automatically on startup,
     so once a migration exists it will apply on every run.

   - **Option B — Run the plain SQL script:**
     Run `CommunityComplaintDB.sql` (in the parent folder) in SSMS or
     `sqlcmd` against your SQL Server instance. If you use this option,
     skip `dotnet ef database update` — but the app's own EF model
     won't have migration history, so Option A is cleaner if you plan
     to change the schema later.

3. **Run the app:**
   ```bash
   dotnet restore
   dotnet run
   ```

4. **Default admin login** (seeded automatically on first run):
   - Username: `admin`
   - Password: `Admin@123`

   Change this password (add a change-password feature, or update the
   `Admins` table with a new hash) before using this in anything beyond
   an academic/demo setting.

## Project structure
- `Models/` — `User`, `Complaint`, `Admin` (mirrors the report's ER diagram)
- `Data/ApplicationDbContext.cs` — EF Core context
- `Data/DbInitializer.cs` — applies migrations + seeds the default admin
- `Controllers/ComplaintController.cs` — registration, submission, tracking
- `Controllers/AdminController.cs` — admin login + dashboard
- `Views/` — Razor views (Bootstrap 5)
- `wwwroot/uploads/` — uploaded complaint photos

## Design & recent additions
- Visual theme reskinned to a dark background with a green accent
  (translucent panel cards, pill-shaped buttons, Manrope/Inter type)
  based on a supplied dashboard design concept.
- The home page includes a short empathy message for citizens landing
  on the site: *"Sorry that you are facing a civic issue. We are here
  to help you report it easily."*
- The complaint category dropdown includes an **Other** option; picking
  it reveals a "Please specify" text field, and that free-text value is
  stored as the complaint's category (no schema change needed —
  `Category` is already a plain `VARCHAR(50)`).
- The complaint form's Location field has a **Use GPS** button that
  calls the browser's Geolocation API, reverse-geocodes the coordinates
  client-side (via a free, keyless API), and fills in a human-readable
  address — falling back to raw coordinates if the lookup fails. The
  citizen can still type or edit the location manually.

## Notes / things to harden before real deployment
- Admin passwords are hashed with `Microsoft.AspNetCore.Identity.PasswordHasher`
  — never stored in plain text.
- File uploads are restricted to `.jpg`/`.jpeg`/`.png`, max 2 MB.
- Session-based login is used for both citizens and admins (simple,
  matches the report's scope) rather than full ASP.NET Core Identity
  with cookie auth — swap in Identity if you need roles, MFA, etc.
  (the report itself lists MFA as a known limitation/future item).
- Add HTTPS enforcement, anti-forgery tokens (already applied to POSTs),
  and rate-limiting on the tracking endpoint in production.
