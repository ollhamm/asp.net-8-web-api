using MailKit.Net.Smtp;
using MimeKit;


namespace aspnetcoreapi.Services
{
    public class EmailServices
    {
        private readonly IConfiguration _config;
        public EmailServices(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken ct = default)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _config["EmailSettings:SenderName"],
                _config["EmailSettings:SenderEmail"]
            ));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _config["EmailSettings:SmtpServer"],
                int.Parse(_config["EmailSettings:SmtpPort"]!),
                MailKit.Security.SecureSocketOptions.StartTls,
                ct
            );
            await client.AuthenticateAsync(
                _config["EmailSettings:SenderEmail"],
                _config["EmailSettings:Password"],
                ct
            );
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
        }
    }
}