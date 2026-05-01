using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Results.Auth
{
    public class RefreshTokenResult
    {
        public bool Success { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
        public List<string>? Errors { get; set; }
    }
}
