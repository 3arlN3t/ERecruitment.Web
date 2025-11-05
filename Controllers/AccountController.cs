using ERecruitment.Web.Services;
using ERecruitment.Web.ViewModels;
using ERecruitment.Web.Models;
using ERecruitment.Web.Data;
using ERecruitment.Web.Notifications;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace ERecruitment.Web.Controllers;

public class AccountController : Controller
{
    private readonly IApplicantManagementService _applicantService;
    private readonly ICurrentApplicant _currentApplicant;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ApplicationDbContext _context;
    private readonly IEmailSender _emailSender;

    public AccountController(
        IApplicantManagementService applicantService,
        ICurrentApplicant currentApplicant,
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ApplicationDbContext context,
        IEmailSender emailSender)
    {
        _applicantService = applicantService;
        _currentApplicant = currentApplicant;
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _emailSender = emailSender;
    }

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

        // Create Identity user
        var existingUser = await _userManager.FindByEmailAsync(model.Email.Trim());
        if (existingUser is not null)
        {
            ModelState.AddModelError(string.Empty, "An account with this email already exists.");
            return View(model);
        }

        var identityUser = new IdentityUser { UserName = model.Email.Trim(), Email = model.Email.Trim(), EmailConfirmed = true };
        var identityResult = await _userManager.CreateAsync(identityUser, model.Password);
        if (!identityResult.Succeeded)
        {
            foreach (var error in identityResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        var result = await _applicantService.RegisterApplicantAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Unable to register at this time.");
            return View(model);
        }

        await _currentApplicant.SetAsync(result.Applicant!.Id);
        await _signInManager.SignInAsync(identityUser, isPersistent: false);
        TempData["Flash"] = "Account created successfully. Welcome to the eRecruitment portal.";
        return RedirectToAction("Dashboard", "Applicant");
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var identityUser = await _userManager.FindByEmailAsync(model.Email.Trim());
        if (identityUser is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        var signIn = await _signInManager.PasswordSignInAsync(identityUser, model.Password, isPersistent: false, lockoutOnFailure: true);
        if (!signIn.Succeeded)
        {
            ModelState.AddModelError(string.Empty, signIn.IsLockedOut ? "Account locked due to failed attempts. Try again later." : "Invalid email or password.");
            return View(model);
        }

        // Admin shortcut by email to guarantee dashboard access
        const string adminEmail = "admin@example.com";
        if (string.Equals(identityUser.Email, adminEmail, StringComparison.OrdinalIgnoreCase))
        {
            if (!await _userManager.IsInRoleAsync(identityUser, "Admin"))
            {
                await _userManager.AddToRoleAsync(identityUser, "Admin");
            }
            TempData["Flash"] = "Signed in as Admin.";
            return RedirectToAction("Dashboard", "Admin");
        }

        if (await _userManager.IsInRoleAsync(identityUser, "Admin"))
        {
            TempData["Flash"] = "Signed in as Admin.";
            return RedirectToAction("Dashboard", "Admin");
        }

        var applicant = await _applicantService.FindApplicantByEmailAsync(model.Email.Trim());
        if (applicant is null)
        {
            // Bridge: if Identity user exists but domain profile doesn't, create minimal profile
            var reg = await _applicantService.RegisterApplicantAsync(new RegisterViewModel
            {
                Email = model.Email.Trim(),
                Password = model.Password,
                ConfirmPassword = model.Password,
                SaIdNumber = "0000000000000"
            });
            if (reg.Success)
            {
                await _currentApplicant.SetAsync(reg.Applicant!.Id);
            }
        }
        else
        {
            await _currentApplicant.SetAsync(applicant.Id);
        }

        TempData["Flash"] = "Signed in successfully.";
        return RedirectToAction("Dashboard", "Applicant");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        _currentApplicant.Clear();
        await _signInManager.SignOutAsync();
        TempData["Flash"] = "You have been signed out.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email.Trim());
        if (user == null)
        {
            // Don't reveal that the user doesn't exist for security
            TempData["Flash"] = "If an account exists with that email, a password reset link has been sent.";
            return RedirectToAction("Login");
        }

        // Generate secure token
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        // Save to database
        var passwordReset = new PasswordReset
        {
            Email = model.Email.Trim(),
            Token = token,
            ExpiresAtUtc = DateTime.UtcNow.AddHours(1) // Token valid for 1 hour
        };

        _context.PasswordResets.Add(passwordReset);
        await _context.SaveChangesAsync();

        // Generate reset URL
        var resetUrl = Url.Action("ResetPassword", "Account",
            new { token = token, email = model.Email },
            protocol: Request.Scheme);

        // Send email
        var emailBody = $@"
            <h2>Password Reset Request</h2>
            <p>Hi,</p>
            <p>You recently requested to reset your password for your eRecruitment account. Click the button below to reset it:</p>
            <p style='margin: 30px 0;'>
                <a href='{resetUrl}' style='background-color: #2563eb; color: white; padding: 12px 24px; text-decoration: none; border-radius: 8px; display: inline-block;'>Reset Password</a>
            </p>
            <p>Or copy and paste this link into your browser:</p>
            <p><a href='{resetUrl}'>{resetUrl}</a></p>
            <p>This link will expire in 1 hour.</p>
            <p>If you didn't request a password reset, please ignore this email or contact support if you have concerns.</p>
            <hr style='margin: 30px 0; border: none; border-top: 1px solid #e5e7eb;'>
            <p style='color: #6b7280; font-size: 0.875rem;'>
                <strong>eRecruitment Portal</strong><br>
                South Africa's public sector recruitment platform
            </p>";

        await _emailSender.SendAsync(model.Email.Trim(), "Reset Your Password", emailBody);

        TempData["Flash"] = "If an account exists with that email, a password reset link has been sent.";
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult ResetPassword(string token, string email)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
        {
            return RedirectToAction("Login");
        }

        var model = new ResetPasswordViewModel
        {
            Token = token,
            Email = email
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Verify token
        var passwordReset = await _context.PasswordResets
            .FirstOrDefaultAsync(pr =>
                pr.Token == model.Token &&
                pr.Email == model.Email.Trim() &&
                !pr.IsUsed &&
                pr.ExpiresAtUtc > DateTime.UtcNow);

        if (passwordReset == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid or expired password reset link.");
            return View(model);
        }

        // Find user
        var user = await _userManager.FindByEmailAsync(model.Email.Trim());
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Unable to reset password.");
            return View(model);
        }

        // Reset password
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, model.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        // Mark token as used
        passwordReset.IsUsed = true;
        passwordReset.UsedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["Flash"] = "Your password has been reset successfully. Please sign in with your new password.";
        return RedirectToAction("Login");
    }
}
