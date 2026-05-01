using System;
using System.Collections.Generic;
using System.Text;

namespace Shared
{
    public class AppConstants
    {
        // ===== Regex Patterns =====
        public static class RegexPatterns
        {
            public const string Email = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            public const string Password = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$";
            public const string PhoneNumber = @"^\+?[1-9][0-9]{7,14}$";
        }



        // ===== Connection String Names =====
        public static class ConnectionStrings
        {
            public const string Default = "DefaultConnection";
            public const string Redis = "Redis";
        }

        // ===== JWT Default Values =====
        public static class JWTDefaults
        {
            public const string Key = "57lbySyDK4PsUxizDWopbscV1VkzF4q8wTyEaxAfqLA";
            public const string Audience = "NotesApiClient";
            public const string Issuer = "NotesApi";
            public const string AccessTokenExpiryMinutes = "60";
            public const string RefreshTokenExpiryDays = "5";
        }
    }
}
