using Microsoft.AspNetCore.Mvc.RazorPages;
using POS.Data;
using POS.Models;
using System.Collections.Generic;
using System.Linq;

namespace POS.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IEnumerable<Product> Products { get; set; } = new List<Product>();

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
            Products = _context.Products.ToList();
        }
    }
}
