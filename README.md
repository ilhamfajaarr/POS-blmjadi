
# POS (Point of Sale) Web Application

Aplikasi **Point of Sale (POS)** berbasis **ASP.NET Core Razor Pages** yang digunakan untuk mengelola menu produk, transaksi, laporan penjualan, serta integrasi notifikasi melalui **Telegram** dan **WhatsApp (Twilio)**.

---

## 📌 Fitur Utama

- 🔐 **Login User** dengan role
- 📦 **Manajemen Produk (Menu Admin)**
  - Tambah produk
  - Edit produk
  - Hapus produk
  - Upload gambar produk
- 📊 **Laporan Transaksi**
  - Filter berdasarkan tanggal
  - Filter berdasarkan nama produk
  - Filter berdasarkan Transaction ID
- 📩 **Notifikasi**
  - Kirim foto ke **Telegram**
  - Kirim pesan WhatsApp menggunakan **Twilio**

---

## 🏗 Arsitektur: MVC

Aplikasi ini menggunakan pola **MVC (Model–View–Controller)**:

### 1. Model
Mengelola data dan koneksi database.
- `Product`
- `Transaction`, `TransactionItem`
- `ApplicationDbContext`

### 2. Controller (PageModel)
Mengelola logika aplikasi dan interaksi user.
- `IndexModel` → Menampilkan & menambah produk
- `EditModel` → Mengedit data produk
- `ReportsModel` → Menampilkan laporan transaksi
- `LoginModel` → Proses autentikasi

### 3. View
Menampilkan antarmuka pengguna menggunakan Razor Pages (`.cshtml`).
- `Index.cshtml`
- `Edit.cshtml`
- `Reports.cshtml`
- `Login.cshtml`

**Alur MVC:**  
User → View → Controller (PageModel) → Model (Database) → Controller → View

---

## ⚙️ Konfigurasi Aplikasi

### 1. Koneksi Database

Pada file `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=LAPTOP-PUUJ7MQU\\SQLEXPRESS;Database=POSDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
````

Digunakan pada `Program.cs`:

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

---

### 2. Konfigurasi Telegram

```json
"Telegram": {
  "BotToken": "YOUR_BOT_TOKEN",
  "ChatId": "YOUR_CHAT_ID"
}
```

Digunakan oleh `TelegramService` untuk mengirim foto transaksi ke Telegram.

---

### 3. Konfigurasi Twilio (WhatsApp)

```json
"Twilio": {
  "AccountSid": "YOUR_ACCOUNT_SID",
  "AuthToken": "YOUR_AUTH_TOKEN",
  "FromWhatsApp": "whatsapp:+14155238886"
}
```

Digunakan oleh `TwilioService` untuk mengirim notifikasi WhatsApp.

---

## 🔧 Dependency Injection

Service yang digunakan didaftarkan di `Program.cs`:

```csharp
builder.Services.AddRazorPages();
builder.Services.AddSession();
builder.Services.AddScoped<TelegramService>();
builder.Services.AddScoped<TwilioService>();
```

---

## 🔐 Session & Role

Aplikasi menggunakan **Session** untuk autentikasi dan otorisasi.

Contoh pengecekan role:

```csharp
if (HttpContext.Session.GetString("Role") != "AdminMenu")
{
    return RedirectToPage("/Login");
}
```

---

## 🖼 Upload Gambar Produk

* File disimpan di folder: `wwwroot/uploads`
* Nama file dibuat unik menggunakan `GUID`
* Path disimpan ke database melalui properti `ImageUrl`

---

## 📊 Laporan Transaksi

Halaman **Reports** menampilkan data transaksi dengan filter:

* **From Date**
* **To Date**
* **Product Name**
* **Transaction ID**

Data diambil melalui query LINQ pada `ReportsModel` dan ditampilkan dalam tabel.

---

## 🚀 Cara Menjalankan Aplikasi

1. Clone repository
2. Pastikan database SQL Server tersedia
3. Jalankan migration / pastikan database `POSDb` sudah ada
4. Konfigurasi:

   * Connection String
   * Telegram Bot
   * Twilio
5. Jalankan project:

   ```bash
   dotnet run
   ```
6. Akses melalui browser:

   ```
   https://localhost:xxxx
   ```

---

## 📌 Catatan

* Folder `wwwroot/uploads` otomatis dibuat jika belum ada.
* Pastikan token Telegram dan Twilio tidak dipublikasikan di repository publik.

---

## 👤 Developer

**Ilham F**
Project: Sistem POS berbasis ASP.NET Core Razor Pages

```

---

