using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using POS.Data;

namespace POS.Pages
{
    public class ReportsModel : PageModel
    {
        // DbContext untuk akses database
        private readonly ApplicationDbContext _context;

        // Constructor: inject DbContext
        public ReportsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // =============================
        // DATA YANG DIKIRIM KE VIEW
        // =============================
        // List ini akan dipakai di halaman reports untuk menampilkan tabel
        // Diinisialisasi kosong supaya tidak null
        public List<ReportRow> ReportData { get; set; } = new List<ReportRow>();


        // GET: HALAMAN REPORTS
        // Parameter filter:
        // from  = tanggal awal
        // to    = tanggal akhir
        // product = nama produk
        // tid   = transaction id

        public IActionResult OnGet(DateTime? from, DateTime? to, string? product, int? tid)
        {

            // CEK LOGIN
  
            // Kalau session Role kosong, berarti belum login → arahkan ke halaman Login
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Role")))
            {
                return RedirectToPage("/Login");
            }

            // QUERY DASAR (JOIN TRANSACTIONS & ITEMS)
            // Ambil data transaksi lalu join dengan item-item di dalam transaksi
            var query = (from t in _context.Transactions
                         join i in _context.TransactionItems
                           on t.Id equals i.TransactionId
                         select new ReportRow
                         {
                             TransactionId = t.Id,
                             Date = t.Date,
                             ProductName = i.ProductName,
                             Quantity = i.Quantity,
                             Price = i.Price,
                             Total = i.Quantity * i.Price
                         }).AsQueryable();

 
            // FILTER BERDASARKAN TANGGAL
            if (from.HasValue)
                query = query.Where(r => r.Date.Date >= from.Value.Date);

            if (to.HasValue)
                query = query.Where(r => r.Date.Date <= to.Value.Date);

 
            // FILTER BERDASARKAN NAMA PRODUK
 
            if (!string.IsNullOrEmpty(product))
                query = query.Where(r => r.ProductName.Contains(product));

            // FILTER BERDASARKAN ID TRANSAKSI
            if (tid.HasValue)
                query = query.Where(r => r.TransactionId == tid.Value);

            // EKSEKUSI QUERY
            // AsNoTracking() → karena ini hanya baca data (lebih ringan & cepat)
            // OrderBy Date → supaya laporan rapi dari yang paling lama ke terbaru
            ReportData = query
                .AsNoTracking()
                .OrderBy(r => r.Date)
                .ToList();

            return Page();
        }

        // DTO / MODEL UNTUK TAMPILAN REPORT
        // Kelas ini hanya untuk menampung data hasil query
        // yang nanti ditampilkan di tabel halaman Reports
        public class ReportRow
        {
            public int TransactionId { get; set; }
            public DateTime Date { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public decimal Total { get; set; }
        }
    }
}
