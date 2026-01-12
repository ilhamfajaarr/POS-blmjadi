using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using POS.Data;
using POS.Models;

namespace POS.Pages.MenuAdmin
{
    public class EditModel : PageModel
    {
        // DbContext untuk akses database (tabel Products)
        private readonly ApplicationDbContext _context;

        // Untuk akses folder wwwroot (upload gambar)
        private readonly IWebHostEnvironment _environment;

        // Constructor: inject DbContext dan Environment
        public EditModel(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // =============================
        // DATA PRODUK YANG DIEDIT
        // =============================
        // BindProperty: otomatis ambil data dari form
        [BindProperty]
        public Product Product { get; set; } = new();

        // =============================
        // FILE GAMBAR (UPLOAD)
        // =============================
        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        // =============================
        // GET: HALAMAN EDIT PRODUK
        // =============================
        public IActionResult OnGet(int id)
        {
            // Cek role user → hanya AdminMenu yang boleh akses
            if (HttpContext.Session.GetString("Role") != "AdminMenu")
            {
                return RedirectToPage("/Login");
            }

            // Ambil data produk berdasarkan id
            Product = _context.Products.FirstOrDefault(p => p.Id == id);

            // Kalau produk tidak ditemukan, balik ke halaman index
            if (Product == null) return RedirectToPage("Index");

            return Page();
        }

        // =============================
        // POST: SIMPAN PERUBAHAN PRODUK
        // =============================
        public IActionResult OnPost()
        {
            // Validasi model (required field, dll)
            if (!ModelState.IsValid) return Page();

            // =============================
            // PROSES UPLOAD GAMBAR (JIKA ADA)
            // =============================
            if (ImageFile != null)
            {
                // Tentukan folder upload: wwwroot/uploads
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);

                // Buat nama file unik supaya tidak bentrok
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Simpan file ke folder uploads
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ImageFile.CopyTo(stream);
                }

                // Simpan path gambar ke database
                Product.ImageUrl = "/uploads/" + fileName;
            }

            // =============================
            // UPDATE DATA KE DATABASE
            // =============================
            _context.Products.Update(Product);
            _context.SaveChanges();

            // Setelah sukses, kembali ke halaman index produk
            return RedirectToPage("Index");
        }
    }
}
