using System.Net.Http.Headers;

public class TelegramService
{
    private readonly string _botToken;
    private readonly string _chatId;

    // Ambil BotToken dan ChatId dari konfigurasi (appsettings.json)
    public TelegramService(IConfiguration config)
    {
        _botToken = config["Telegram:BotToken"];
        _chatId = config["Telegram:ChatId"];
    }

    // Mengirim foto ke Telegram dengan caption (opsional)
    public async Task SendPhotoAsync(string imagePath, string caption = "")
    {
        using var client = new HttpClient();
        using var form = new MultipartFormDataContent();

        // Parameter wajib untuk Telegram API
        form.Add(new StringContent(_chatId), "chat_id");
        form.Add(new StringContent(caption), "caption");

        // Baca file gambar dan ubah ke byte
        var bytes = await File.ReadAllBytesAsync(imagePath);
        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");

        // Tambahkan file foto ke request
        form.Add(fileContent, "photo", Path.GetFileName(imagePath));

        // Kirim request ke endpoint Telegram API
        await client.PostAsync(
            $"https://api.telegram.org/bot{_botToken}/sendPhoto",
            form
        );
    }
}
