using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.SeedData
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            foreach (UserRole roleEnum in Enum.GetValues(typeof(UserRole)))
            {
                var roleName = roleEnum.ToString();
                var roleExists = await roleManager.RoleExistsAsync(roleName);

                if (!roleExists)
                {
                    var description = roleEnum switch
                    {
                        UserRole.User => "Standard user with basic access",
                        UserRole.Admin => "Administrator with full access",
                        UserRole.SuperAdmin => "Super administrator with complete system access",
                        UserRole.Support => "Support staff with limited access",
                        UserRole.Employee => "Employee with regular access",
                        _ => $"{roleName} role"
                    };

                    var role = new ApplicationRole
                    {
                        Name = roleName,
                        Description = description,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    await roleManager.CreateAsync(role);
                }
            }
        }
    }
}