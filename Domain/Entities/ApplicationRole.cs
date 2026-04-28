using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        public ApplicationRole() : base()
        {
            Id = Guid.NewGuid();
        }

        public ApplicationRole(string roleName) : base(roleName)
        {
            Id = Guid.NewGuid();
        }

        public ApplicationRole(string roleName, string description) : base(roleName)
        {
            Id = Guid.NewGuid();
            Description = description;
            CreatedAt = DateTime.UtcNow;
        }

        // خواص اضافی
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation property (اختیاری - برای رابطه با کاربران)
        public virtual ICollection<IdentityUserRole<Guid>> UserRoles { get; set; } = new List<IdentityUserRole<Guid>>();
        public virtual ICollection<IdentityRoleClaim<Guid>> RoleClaims { get; set; } = new List<IdentityRoleClaim<Guid>>();
    }
}
