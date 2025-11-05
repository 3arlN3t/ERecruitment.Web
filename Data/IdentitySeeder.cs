using Microsoft.AspNetCore.Identity;
using ERecruitment.Web.Models;
using ERecruitment.Web.Services;

namespace ERecruitment.Web.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Seed Admin Role and User
        var adminRole = "Admin";
        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(adminRole));
        }

        var adminEmail = "admin@example.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            var create = await userManager.CreateAsync(adminUser, "Admin1234!");
            if (create.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, adminRole);
            }
        }
        else
        {
            if (!await userManager.IsInRoleAsync(adminUser, adminRole))
            {
                await userManager.AddToRoleAsync(adminUser, adminRole);
            }
        }

        // Seed Test Applicants
        await SeedTestApplicantAsync(userManager, context, "john.smith@test.com", "Test1234!", "John", "Smith", "9001015009087");
        await SeedTestApplicantAsync(userManager, context, "sarah.jones@test.com", "Test1234!", "Sarah", "Jones", "8505205009088");
        await SeedTestApplicantAsync(userManager, context, "david.williams@test.com", "Test1234!", "David", "Williams", "7803105009089");
    }

    private static async Task SeedTestApplicantAsync(
        UserManager<IdentityUser> userManager,
        ApplicationDbContext context,
        string email,
        string password,
        string firstName,
        string lastName,
        string saIdNumber)
    {
        // Check if identity user already exists
        var identityUser = await userManager.FindByEmailAsync(email);
        if (identityUser == null)
        {
            identityUser = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(identityUser, password);
            if (!result.Succeeded)
            {
                return; // Skip if identity creation failed
            }
        }

        // Check if applicant profile already exists
        var existingApplicant = context.Applicants.FirstOrDefault(a => a.Email == email);
        if (existingApplicant != null)
        {
            return; // Already exists
        }

        // Create applicant profile
        var applicant = new Applicant
        {
            Email = email,
            PasswordHash = "dummy", // Not used, managed by Identity
            Profile = new ApplicantProfile
            {
                FirstName = firstName,
                LastName = lastName,
                SaIdNumber = saIdNumber,
                PhoneNumber = "+27 82 123 4567",
                Location = "Pretoria, Gauteng",
                ContactEmail = email
            },
            EquityDeclaration = new EquityDeclaration
            {
                ConsentGiven = false
            }
        };

        context.Applicants.Add(applicant);
        await context.SaveChangesAsync();
    }
}


