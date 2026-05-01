using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Extensions
{
    public static class UserRoleExtensions
    {
        /// <summary>
        /// دریافت نام نمایشی نقش
        /// </summary>
        public static string GetDisplayName(this UserRole role)
        {
            return role switch
            {
                UserRole.User => "User",
                UserRole.Admin => "Admin",
                UserRole.SuperAdmin => "SuperAdmin",
                UserRole.Support => "Support",
                UserRole.Employee => "Employee",
                _ => role.ToString()
            };
        }

        /// <summary>
        /// دریافت لیست همه نقش‌ها
        /// </summary>
        public static List<UserRole> GetAllRoles()
        {
            return Enum.GetValues(typeof(UserRole))
                .Cast<UserRole>()
                .ToList();
        }

        /// <summary>
        /// دریافت نقش از رشته
        /// </summary>
        public static UserRole? FromString(string roleName)
        {
            return Enum.TryParse<UserRole>(roleName, true, out var role) ? role : null;
        }

        /// <summary>
        /// اعتبارسنجی نقش
        /// </summary>
        public static bool IsValid(string roleName)
        {
            return Enum.TryParse<UserRole>(roleName, true, out _);
        }
    }
}
