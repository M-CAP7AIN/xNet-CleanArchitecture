using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Results.Auth
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public List<string>? Errors { get; set; }
    }
}
