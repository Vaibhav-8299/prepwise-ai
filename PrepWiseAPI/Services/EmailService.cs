using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace PrepWiseAPI.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendOtpEmailAsync(string toEmail, string otpCode, string purpose)
        {
            var subject = purpose == "EmailVerify"
                ? "PrepWise AI - Verify Your Email"
                : "PrepWise AI - Password Reset Code";

            var body = purpose == "EmailVerify"
                ? $@"<h2>Welcome to PrepWise AI! 🚀</h2>
                     <p>Your email verification code is:</p>
                     <h1 style='color: #6C63FF; letter-spacing: 8px;'>{otpCode}</h1>
                     <p>This code expires in <strong>10 minutes</strong>.</p>
                     <p>If you didn't create an account, ignore this email.</p>"
                : $@"<h2>Password Reset Request</h2>
                     <p>Your password reset code is:</p>
                     <h1 style='color: #6C63FF; letter-spacing: 8px;'>{otpCode}</h1>
                     <p>This code expires in <strong>10 minutes</strong>.</p>
                     <p>If you didn't request this, ignore this email.</p>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendScoreEmailAsync(string toEmail, string fullName, int score, string role)
        {
            var subject = "PrepWise AI - Your Mock Interview Score";
            var body = $@"<h2>Hi {fullName}! 🎯</h2>
                         <p>Your mock interview for <strong>{role}</strong> is complete.</p>
                         <h1 style='color: #4CAF50;'>Score: {score}/100</h1>
                         <p>Login to PrepWise AI to see detailed feedback for each question.</p>";

            await SendEmailAsync(toEmail, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var smtpServer = _config["EmailSettings:SmtpServer"]!;
            var smtpPort = int.Parse(_config["EmailSettings:SmtpPort"]!);
            var senderEmail = _config["EmailSettings:SenderEmail"]!;
            var senderPassword = _config["EmailSettings:SenderPassword"]!;
            var senderName = _config["EmailSettings:SenderName"]!;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            message.Body = new TextPart("html")
            {
                Text = htmlBody
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(senderEmail, senderPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
