using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Enums
{
    public enum UserStatus
    {
        /// <summary>
        /// فعال
        /// </summary>
        Active = 1,

        /// <summary>
        /// غیرفعال
        /// </summary>
        Inactive = 2,

        /// <summary>
        /// مسدود شده
        /// </summary>
        Banned = 3,

        /// <summary>
        /// در انتظار تایید
        /// </summary>
        Pending = 4
    }
}
