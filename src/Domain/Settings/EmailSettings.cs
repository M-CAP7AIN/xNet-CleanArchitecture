using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Settings
{
    public class EmailSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool EnableSSL { get; set; }
        public bool UseDefaultCredentials { get; set; }
    }
}
