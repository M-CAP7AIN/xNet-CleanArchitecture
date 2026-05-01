using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body);
        Task<bool> SendEmailWithTemplateAsync(string to, string subject, string templateName, Dictionary<string, string> placeholders);
        Task<bool> SendBulkEmailAsync(List<string> toList, string subject, string body);


    }
}
