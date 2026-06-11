using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace FirstErpSystem.Api.Services;

/*
================================================================
EmailService — Added By Himel Sarkar 09-06-2026
LEARNING FLOW:
- MailKit = .NET এর best email library (free, open source)
- SMTP = Simple Mail Transfer Protocol (email পাঠানোর protocol)
- Gmail SMTP settings:
  Host: smtp.gmail.com
  Port: 587
  Security: StartTls
- Gmail App Password লাগবে (normal password কাজ করে না)
  Google Account → Security → 2FA on → App Passwords
- MimeMessage = email এর structure
  From, To, Subject, Body সব এখানে set করি
================================================================
*/
public class EmailService : IEmailService
{
    //Added By Himel Sarkar 09-06-2026
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string toEmail, string toName, string subject, string body)
    {
        /*
        LEARNING:
        MimeMessage = email এর একটা object
        BodyBuilder = HTML + plain text body তৈরি করে
        SmtpClient = Gmail server-এর সাথে connect করে email পাঠায়
        */
        var email = new MimeMessage();

        // Sender info — appsettings.json থেকে আসবে
        email.From.Add(new MailboxAddress(
            _config["EmailSettings:SenderName"],
            _config["EmailSettings:SenderEmail"]
        ));

        // Receiver info
        email.To.Add(new MailboxAddress(toName, toEmail));
        email.Subject = subject;

        // Email body — HTML format
        var builder = new BodyBuilder
        {
            HtmlBody = body
        };
        email.Body = builder.ToMessageBody();

        // Connect to Gmail SMTP and send
        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(
            _config["EmailSettings:SmtpHost"],
            int.Parse(_config["EmailSettings:SmtpPort"]!),
            SecureSocketOptions.StartTls
        );
        await smtp.AuthenticateAsync(
            _config["EmailSettings:SenderEmail"],
            _config["EmailSettings:AppPassword"]
        );
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
    //End By Himel Sarkar 09-06-2026
}
