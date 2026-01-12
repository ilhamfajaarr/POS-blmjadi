using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using POS.Data;
using System;
using System.Data;
using System.Linq;

namespace POS.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public LoginModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Username { get; set; } = default!;

        [BindProperty]
        public string Password { get; set; } = default!;

        public string ErrorMessage { get; set; } = string.Empty;

        public IActionResult OnPost()
        {
            // Ambil user dari database
            var user = _context.Users
                               .FirstOrDefault(u => u.Username == Username
                                                 && u.Password == Password);

            if (user == null)
            {
                ErrorMessage = "Username atau password salah";
                return Page();
            }

            // Simpan session
            HttpContext.Session.SetString("Role", user.Role);

            // Redirect sesuai role
            if (user.Role == "Admin")
                return RedirectToPage("/Index"); // Halaman POS

            if (user.Role == "Owner")
                return RedirectToPage("/Reports/Reports"); // Halaman laporan

            if (user.Role == "AdminMenu")
                return RedirectToPage("/MenuAdmin/Index");


            return Page();
        }
    }
}
