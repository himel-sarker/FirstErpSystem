namespace FirstErpSystem.Api.Services;

/*
================================================================
SmsService — Added By Himel Sarker 09-06-2026
LEARNING FLOW:
- এখন Development mode-এ SMS console-এ log হবে
- Production-এ BulkSMS BD API দিয়ে replace করব
- এই pattern কে "Strategy Pattern" বলে —
  implementation আলাদা রেখে interface same থাকে
- Real BulkSMS BD API:
  GET https://bulksmsbd.net/api/smsapi
  ?api_key=KEY&type=text&number=8801X&senderid=ID&message=MSG
================================================================
*/
public class SmsService : ISmsService
{
    //Added By Himel Sarker 09-06-2026
    private readonly IConfiguration _config;
    private readonly ILogger<SmsService> _logger;

    public SmsService(IConfiguration config, ILogger<SmsService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendSmsAsync(string phoneNumber, string message)
    {
        /*
        LEARNING:
        Development mode-এ real SMS না পাঠিয়ে
        console-এ log করি — testing সহজ হয়
        Production-এ IsProduction() check করে real API call করব
        */
        var formattedPhone = phoneNumber.StartsWith("0")
            ? "880" + phoneNumber.Substring(1)
            : phoneNumber;

        // Development: log to console
        _logger.LogInformation(
            "SMS [DEV MODE] → To: {Phone} | Message: {Message}",
            formattedPhone, message
        );

        // Console-এ সুন্দরভাবে দেখাবে
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("─────────────────────────────────");
        Console.WriteLine($"📱 SMS SENT (Dev Mode)");
        Console.WriteLine($"   To:      {formattedPhone}");
        Console.WriteLine($"   Message: {message}");
        Console.WriteLine("─────────────────────────────────");
        Console.ResetColor();

        /*
        PRODUCTION CODE (BulkSMS BD):
        var apiKey   = _config["SmsSettings:ApiKey"];
        var senderId = _config["SmsSettings:SenderId"];
        var encodedMsg = Uri.EscapeDataString(message);
        var url = $"https://bulksmsbd.net/api/smsapi" +
                  $"?api_key={apiKey}&type=text" +
                  $"&number={formattedPhone}" +
                  $"&senderid={senderId}" +
                  $"&message={encodedMsg}";
        var client = _httpClientFactory.CreateClient();
        await client.GetAsync(url);
        */

        await Task.CompletedTask;
    }
    //End By Himel Sarker 09-06-2026
}
