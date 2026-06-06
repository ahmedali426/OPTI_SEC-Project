using Microsoft.AspNetCore.Identity;
using Opti_Sec_Backend.Entities;

namespace Opti_Sec_Backend.Abstractions.Consts;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, IConfiguration config)
    {
        using var scope = serviceProvider.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        if (!await roleManager.RoleExistsAsync(DefaultRoles.Admin))
        {
            await roleManager.CreateAsync(new ApplicationRole
            {
                Id = DefaultRoles.AdminRoleId,
                Name = DefaultRoles.Admin,
                NormalizedName = DefaultRoles.Admin.ToUpper()
            });
        }

        if (!await roleManager.RoleExistsAsync(DefaultRoles.Member))
        {
            await roleManager.CreateAsync(new ApplicationRole
            {
                Id = DefaultRoles.MemberRoleId,
                Name = DefaultRoles.Member,
                NormalizedName = DefaultRoles.Member.ToUpper(),
                IsDefault = true
            });
        }

        var admin = await userManager.FindByEmailAsync(DefaultUsers.AdminEmail);

        if (admin is null)
        {
            var password = config["DefaultAdmin:Password"];

            if (string.IsNullOrEmpty(password))
                throw new Exception("Admin password not found in secrets");

            admin = new ApplicationUser
            {
                Id = DefaultUsers.AdminId,
                Email = DefaultUsers.AdminEmail,
                UserName = DefaultUsers.AdminEmail,
                FName = "Opti_Sec",
                LName = "Admin",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, password);

            if (!result.Succeeded)
                throw new Exception("Failed to create admin");
        }

        if (!await userManager.IsInRoleAsync(admin, DefaultRoles.Admin))
        {
            await userManager.AddToRoleAsync(admin, DefaultRoles.Admin);
        }
    }
}
