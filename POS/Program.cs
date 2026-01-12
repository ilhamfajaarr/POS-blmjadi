using Microsoft.EntityFrameworkCore;
using POS.Data;

var builder = WebApplication.CreateBuilder(args);

// Konfigurasi koneksi database (SQL Server)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Daftarkan Razor Pages
builder.Services.AddRazorPages();

// Aktifkan session (untuk login & role)
builder.Services.AddSession();

// Registrasi service eksternal
builder.Services.AddScoped<TelegramService>();
builder.Services.AddScoped<TwilioService>();

var app = builder.Build();

// Konfigurasi error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseDeveloperExceptionPage(); // (sebenarnya ini biasanya hanya untuk development)
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Aktifkan session
app.UseSession();

app.UseAuthorization();

// Mapping Razor Pages
app.MapRazorPages();

app.Run();
