namespace FirstErpSystem.Api.Services;

/*
================================================================
IEmailService Interface — Added By Himel Sarkar 09-06-2026
LEARNING FLOW:
- Interface = contract, বলে কী কী method থাকবে
- Actual implementation আলাদা class-এ (EmailService.cs)
- কেন Interface? → Testing-এ fake implementation দেওয়া যায়
- Dependency Injection: Program.cs-এ register করলে
  Controller-এ constructor-এ inject করা যায়
================================================================
*/
public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string toName, string subject, string body);
}
