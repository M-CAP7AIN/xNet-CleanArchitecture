using Domain.Interfaces;
using Domain.Settings;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;


namespace Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly string _templatesPath;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;

            // مسیر پوشه قالب‌های ایمیل (در پروژه Api)
            _templatesPath = Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates");

            // ایجاد پوشه اگر وجود ندارد
            if (!Directory.Exists(_templatesPath))
            {
                Directory.CreateDirectory(_templatesPath);
            }
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var message = new MimeMessage();

                // فرستنده
                message.From.Add(new MailboxAddress(
                    _emailSettings.SenderName,
                    _emailSettings.SenderEmail));

                // گیرنده
                message.To.Add(MailboxAddress.Parse(to));

                // موضوع
                message.Subject = subject;

                // بدنه ایمیل (HTML)
                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = body,
                    TextBody = StripHtml(body) // نسخه متنی ساده
                };

                message.Body = bodyBuilder.ToMessageBody();

                // ارسال ایمیل
                using var client = new SmtpClient();

                // اتصال به سرور
                await client.ConnectAsync(
                    _emailSettings.Host,
                    _emailSettings.Port,
                    _emailSettings.EnableSSL ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);

                // احراز هویت (اختیاری)
                if (!_emailSettings.UseDefaultCredentials)
                {
                    await client.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.Password);
                }

                // ارسال
                await client.SendAsync(message);

                // قطع اتصال
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                // لاگ خطا
                Console.WriteLine($"Email sending failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendEmailWithTemplateAsync(
            string to,
            string subject,
            string templateName,
            Dictionary<string, string> placeholders)
        {
            try
            {
                // خواندن فایل قالب
                var templatePath = Path.Combine(_templatesPath, $"{templateName}.html");

                if (!File.Exists(templatePath))
                {
                    throw new FileNotFoundException($"Template {templateName} not found");
                }

                var templateContent = await File.ReadAllTextAsync(templatePath);

                // جایگزینی placeholders
                var body = templateContent;
                foreach (var placeholder in placeholders)
                {
                    body = body.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
                }

                // ارسال ایمیل
                return await SendEmailAsync(to, subject, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Template email failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendBulkEmailAsync(List<string> toList, string subject, string body)
        {
            try
            {
                var message = new MimeMessage();

                message.From.Add(new MailboxAddress(
                    _emailSettings.SenderName,
                    _emailSettings.SenderEmail));

                // اضافه کردن همه گیرندگان
                foreach (var to in toList)
                {
                    message.To.Add(MailboxAddress.Parse(to));
                }

                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = body,
                    TextBody = StripHtml(body)
                };

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();

                await client.ConnectAsync(
                    _emailSettings.Host,
                    _emailSettings.Port,
                    _emailSettings.EnableSSL ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);

                if (!_emailSettings.UseDefaultCredentials)
                {
                    await client.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.Password);
                }

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Bulk email failed: {ex.Message}");
                return false;
            }
        }

        // متد کمکی برای حذف تگ‌های HTML
        private static string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;

            return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
        }
    }
}
