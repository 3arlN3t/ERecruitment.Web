using ERecruitment.Web.Services;
using ERecruitment.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ERecruitment.Web.Controllers;

public class AdminController : Controller
{
    private readonly IAdministrationService _adminService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _config;

    public AdminController(
        IAdministrationService adminService,
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration config)
    {
        _adminService = adminService;
        _userManager = userManager;
        _roleManager = roleManager;
        _config = config;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public IActionResult Login(AdminLoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Get the correct access code from configuration
        // You can set this via:
        // 1. User Secrets during development: dotnet user-secrets set "AdminAccessCode" "your-code-here"
        // 2. appsettings.json (not recommended for production)
        // 3. Environment variables: ADMIN_ACCESS_CODE
        // 4. Azure Key Vault or other secure configuration providers
        var correctCode = _config["AdminAccessCode"] ?? _config["Admin:AccessCode"];
        
        if (string.IsNullOrEmpty(correctCode))
        {
            ModelState.AddModelError("AccessCode", "Access code configuration is not set. Contact your administrator.");
            return View(model);
        }

        // Validate the access code (case-sensitive)
        if (model.AccessCode != correctCode)
        {
            ModelState.AddModelError("AccessCode", "The access code you entered is incorrect. Please try again.");
            return View(model);
        }

        // Store a flag in session to indicate admin has provided valid access code
        HttpContext.Session.SetString("AdminAccessGranted", DateTime.UtcNow.ToString("O"));
        
        TempData["Flash"] = "Welcome! Access granted.";
        return RedirectToAction(nameof(Dashboard));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var model = await _adminService.BuildAdminDashboardAsync();
        return View(model);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkReject(AdminBulkRejectViewModel model)
    {
        model.SelectedApplicationIds ??= new List<Guid>();
        model.SelectedApplicationIds = model.SelectedApplicationIds.Distinct().ToList();
        if (!ModelState.IsValid)
        {
            TempData["Flash"] = "Please select at least one application and provide a rejection message.";
            return RedirectToAction(nameof(Dashboard));
        }

        var result = await _adminService.BulkRejectApplicationsAsync(model);
        TempData["Flash"] = result.UpdatedCount == 0
            ? "No applications were updated."
            : $"Rejected {result.UpdatedCount} application(s) and queued personalised notifications.";
        return RedirectToAction(nameof(Dashboard));
    }
}
