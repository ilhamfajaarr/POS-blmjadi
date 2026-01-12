using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using POS.Data;
using POS.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.IO;
using SkiaSharp;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace POS.Pages
{
    // Menonaktifkan validasi AntiForgeryToken (biasanya untuk API / AJAX)
    [IgnoreAntiforgeryToken]
    public class IndexModel : PageModel
    {
        // Database context untuk akses tabel (Products, Transactions, dll)
        private readonly ApplicationDbContext _context;

        // Untuk membaca konfigurasi (Telegram, Twilio, dll dari appsettings.json)
        private readonly IConfiguration _configuration;

        // Data produk yang akan ditampilkan di halaman POS
        public IEnumerable<Product> Products { get; set; }

        // Constructor: Dependency Injection DbContext dan Configuration
        public IndexModel(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // =============================
        // GET: HALAMAN POS
        // =============================
        public IActionResult OnGet()
        {
            // Cek apakah user sudah login (session Role ada atau tidak)
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Role")))
                return RedirectToPage("/Login");

            // Ambil semua produk dari database
            Products = _context.Products.ToList();
            return Page();
        }

        // =============================
        // SIMPAN TRANSAKSI
        // =============================
        [HttpPost]
        public async Task<IActionResult> OnPostSaveTransactionAsync([FromBody] TransactionRequest request)
        {
            // Validasi: pastikan data transaksi tidak kosong
            if (request == null || request.Items == null || request.Items.Count == 0)
                return BadRequest("Data transaksi kosong");

            
            // HITUNG TOTAL
            
            // Subtotal = jumlah (harga x qty)
            decimal subtotal = request.Items.Sum(i => i.Price * i.Quantity);

            // Pajak PPN 10%
            decimal tax = subtotal * 0.10m;

            // Grand total
            decimal grandTotal = subtotal + tax;

            
            // SIMPAN KE DATABASE
            
            var transaction = new Transaction
            {
                TotalAmount = grandTotal,
                Items = request.Items.Select(i => new TransactionItem
                {
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList()
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            
            // GENERATE STRUK DALAM BENTUK GAMBAR
            
            string imagePath = GenerateReceiptImage(request, subtotal, tax, grandTotal);

            
            // KIRIM FOTO STRUK KE TELEGRAM (ADMIN)
           
            await SendTelegramImage(imagePath);

            
            // KIRIM STRUK TEKS KE WHATSAPP (CUSTOMER)
            
            if (!string.IsNullOrEmpty(request.WhatsAppNumber))
            {
                string receiptText = BuildReceiptText(request, subtotal, tax, grandTotal);
                await SendWhatsAppText(request.WhatsAppNumber, receiptText);
            }

            // Response sukses ke frontend
            return new JsonResult(new { success = true });
        }

        
        // GENERATE STRUK IMAGE (SKIASHARP)
        private string GenerateReceiptImage(TransactionRequest data, decimal subtotal, decimal tax, decimal grandTotal)
        {
            // Tentukan folder penyimpanan struk di wwwroot/receipts
            string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "receipts");
            Directory.CreateDirectory(folder);

            // Nama file berdasarkan timestamp agar unik
            string filePath = Path.Combine(folder, $"receipt_{DateTime.Now.Ticks}.png");

            int width = 380;
            int height = 600;

            // Buat canvas gambar
            using var surface = SKSurface.Create(new SKImageInfo(width, height));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);

            // Style teks judul
            var paintTitle = new SKPaint
            {
                TextSize = 22,
                IsAntialias = true,
                Color = SKColors.Black,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };

            // Style teks biasa
            var paintText = new SKPaint
            {
                TextSize = 14,
                IsAntialias = true,
                Color = SKColors.Black,
                Typeface = SKTypeface.FromFamilyName("Arial")
            };

            float y = 30;

            // Judul struk
            canvas.DrawText("STRUK PEMBELIAN", 90, y, paintTitle);
            y += 35;

            // Tanggal transaksi
            canvas.DrawText($"Tanggal: {DateTime.Now:dd/MM/yyyy HH:mm}", 10, y, paintText);
            y += 20;

            // Nomor WhatsApp customer (jika ada)
            if (!string.IsNullOrEmpty(data.WhatsAppNumber))
            {
                canvas.DrawText($"WA: {data.WhatsAppNumber}", 10, y, paintText);
                y += 20;
            }

            // Garis pemisah
            y += 10;
            canvas.DrawLine(10, y, 360, y, paintText);
            y += 20;

            // Daftar item yang dibeli
            foreach (var item in data.Items)
            {
                canvas.DrawText($"{item.ProductName} x{item.Quantity}", 10, y, paintText);
                canvas.DrawText($"Rp {(item.Price * item.Quantity):N0}", 220, y, paintText);
                y += 20;
            }

            // Garis pemisah
            y += 10;
            canvas.DrawLine(10, y, 360, y, paintText);
            y += 25;

            // Subtotal, pajak, dan total
            canvas.DrawText($"Subtotal : Rp {subtotal:N0}", 10, y, paintText); y += 20;
            canvas.DrawText($"PPN 10%  : Rp {tax:N0}", 10, y, paintText); y += 25;
            canvas.DrawText($"TOTAL    : Rp {grandTotal:N0}", 10, y, paintTitle);

            // Simpan gambar ke file
            using var image = surface.Snapshot();
            using var dataImg = image.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = System.IO.File.OpenWrite(filePath);
            dataImg.SaveTo(stream);

            return filePath;
        }

        // KIRIM FOTO STRUK KE TELEGRAM (ADMIN)
      
        private async Task SendTelegramImage(string imagePath)
        {
            // Ambil token bot dan chat id dari appsettings.json
            string botToken = _configuration["Telegram:BotToken"];
            string chatId = _configuration["Telegram:ChatId"];

            // Jika konfigurasi belum diisi, hentikan proses
            if (string.IsNullOrEmpty(botToken) || string.IsNullOrEmpty(chatId))
                return;

            using var client = new HttpClient();
            var url = $"https://api.telegram.org/bot{botToken}/sendPhoto";

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(chatId), "chat_id");

            // Ambil file struk
            var fileStream = System.IO.File.OpenRead(imagePath);
            form.Add(new StreamContent(fileStream), "photo", "receipt.png");

            // Kirim ke Telegram
            await client.PostAsync(url, form);
        }

        // BENTUK TEKS STRUK UNTUK WHATSAPP
    
        private string BuildReceiptText(TransactionRequest request, decimal subtotal, decimal tax, decimal grandTotal)
        {
            var text = " *STRUK PEMBELIAN*\n";
            text += $"Tanggal: {DateTime.Now:dd/MM/yyyy HH:mm}\n\n";

            // daftar item
            foreach (var item in request.Items)
            {
                text += $"{item.ProductName} x{item.Quantity} = Rp {(item.Price * item.Quantity):N0}\n";
            }

            // Total harga
            text += $"\nSubtotal : Rp {subtotal:N0}";
            text += $"\nPPN 10%  : Rp {tax:N0}";
            text += $"\nTOTAL    : Rp {grandTotal:N0}";
            text += "\n\nTerima kasih sudah makan di Ngopite🙏";

            return text;
        }

        
        // KIRIM TEKS STRUK KE WHATSAPP (TWILIO)
        
        private async Task SendWhatsAppText(string customerNumber, string messageText)
        {
            // ambil kredensial twilio
            var accountSid = _configuration["Twilio:AccountSid"];
            var authToken = _configuration["Twilio:AuthToken"];
            var fromNumber = _configuration["Twilio:FromWhatsApp"];

            // kalo konfigurasi belum lengkap, hentikan proses
            if (string.IsNullOrEmpty(accountSid) || string.IsNullOrEmpty(authToken) || string.IsNullOrEmpty(fromNumber))
                return;

            // Inisialisasi Twilio
            TwilioClient.Init(accountSid, authToken);

            // format nomer wa
            string to = "whatsapp:+" + customerNumber;

            // kirim wa
            var message = MessageResource.Create(
                from: new PhoneNumber(fromNumber),
                to: new PhoneNumber(to),
                body: messageText
            );
        }
    }

 
    // buat menerima data dari frontend

    public class TransactionRequest
    {
        public decimal TotalAmount { get; set; }
        public string WhatsAppNumber { get; set; }
        public List<ItemDto> Items { get; set; }
    }

    public class ItemDto
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
