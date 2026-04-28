using Domain.Entities;
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

            var roles = new List<ApplicationRole>
            {
                new ApplicationRole("Admin", "Full system access"),
                new ApplicationRole("User", "Standard user access"),
                new ApplicationRole("Manager", "Management access")
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role.Name ?? string.Empty))
                {
                    await roleManager.CreateAsync(role);
                }
            }
        }
    }
}