using EatIT.Core.Interface;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Core.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public SmtpEmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            var email = _config.GetSection("Email");
            var host = email["SmtpHost"];
            var port = int.Parse(email["SmtpPort"] ?? "587");
            var enableSsl = bool.Parse(email["EnableSsl"] ?? "true");
            var user = email["UserName"];
            var pass = email["Password"];
            var from = email["From"] ?? user;

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(user, pass)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(from),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(to);

            await client.SendMailAsync(message);
        }
    }
}
