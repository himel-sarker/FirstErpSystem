namespace FirstErpSystem.Api.Services;

/*
================================================================
ISmsService Interface — Added By Himel Sarkar 09-06-2026
LEARNING FLOW:
- Email এর মতোই — Interface আলাদা, Implementation আলাদা
- Bangladesh এ BulkSMS BD সবচেয়ে popular free trial SMS service
- API call = HttpClient দিয়ে HTTP GET request
- কোনো NuGet package লাগে না — built-in HttpClient যথেষ্ট
================================================================
*/
public interface ISmsService
{
    Task SendSmsAsync(string phoneNumber, string message);
}
