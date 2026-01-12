using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using POS.Data;
using POS.Models;

namespace POS.Pages.MenuAdmin
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public IndexModel(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // List produk untuk ditampilkan di halaman
        public List<Product> Products { get; set; } = new();

        // Data produk baru dari form
        [BindProperty]
        public Product NewProduct { get; set; } = new();

        // File gambar yang di-upload
        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        public IActionResult OnGet()
        {
            // Cek role admin menu
            if (HttpContext.Session.GetString("Role") != "AdminMenu")
            {
                return RedirectToPage("/Login");
            }

            // Ambil semua produk
            Products = _context.Products.ToList();
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                Products = _context.Products.ToList();
                return Page();
            }

            // Upload gambar jika ada
            if (ImageFile != null)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ImageFile.CopyTo(stream);
                }

                // Simpan path gambar ke database
                NewProduct.ImageUrl = "/uploads/" + fileName;
            }

            // Simpan produk baru
            _context.Products.Add(NewProduct);
            _context.SaveChanges();

            return RedirectToPage();
        }

        public IActionResult OnPostDelete(int id)
        {
            // Hapus produk berdasarkan id
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }

            return RedirectToPage();
        }
    }
}
