using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Enums
{
    public enum UserRole
    {
        /// <summary>
        /// کاربر معمولی
        /// </summary>
        User = 1,

        /// <summary>
        /// مدیر
        /// </summary>
        Admin = 2,

        /// <summary>
        /// مدیر ارشد (دسترسی کامل)
        /// </summary>
        SuperAdmin = 3,

        /// <summary>
        /// پشتیبانی
        /// </summary>
        Support = 4,

        /// <summary>
        /// کارمند
        /// </summary>
        Employee = 5
    }
}
