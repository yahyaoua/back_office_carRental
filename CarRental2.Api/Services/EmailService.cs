using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace CarRental.Api.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        // ✅ MÉTHODE PRINCIPALE
        public async Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            Console.WriteLine("ENV = " + Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
            Console.WriteLine("SMTP HOST = " + _config["Email:SmtpHost"]);

            Console.WriteLine("FROM = " + _config["Email:From"]);
            Console.WriteLine("TO = " + toEmail);


            Console.WriteLine("SMTP HOST FROM CONFIG = " + _config["Email:SmtpHost"]);


            var host = _config["Email:SmtpHost"];
            var port = _config["Email:SmtpPort"];
            var username = _config["Email:Username"];
            var password = _config["Email:Password"];
            var from = _config["Email:From"];

            if (string.IsNullOrEmpty(host))
                throw new Exception("❌ Email:SmtpHost is NULL. Check appsettings.json");

            var smtpClient = new SmtpClient
            {
                Host = host,
                Port = int.Parse(port),
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            var message = new MailMessage
            {
                From = new MailAddress(from),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            await smtpClient.SendMailAsync(message);
        }

    }
}
