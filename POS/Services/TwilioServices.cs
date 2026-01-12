using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

public class TwilioService
{
    private readonly IConfiguration _config;

    public TwilioService(IConfiguration config)
    {
        _config = config;

        TwilioClient.Init(
            _config["Twilio:AccountSid"],
            _config["Twilio:AuthToken"]
        );
    }

    // Kirim pesan teks ke WhatsApp
    public async Task SendWhatsAppMessage(string toNumber, string message)
    {
        await MessageResource.CreateAsync(
            from: new PhoneNumber(_config["Twilio:FromWhatsApp"]),
            to: new PhoneNumber("whatsapp:" + toNumber),
            body: message
        );
    }
}
