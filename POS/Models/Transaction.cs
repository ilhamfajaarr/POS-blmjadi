using System;
using System.Collections.Generic;

namespace POS.Models
{
    // Model transaksi: ringkasan sebuah penjualan
    public class Transaction
    {
        // Primary key transaksi
        public int Id { get; set; }

        // Waktu pembuatan transaksi; default di-set ke waktu sekarang
        public DateTime Date { get; set; } = DateTime.Now;

        // Grand total untuk transaksi ini (setelah pajak)
        public decimal TotalAmount { get; set; }

        // Relasi ke detail item
        // Koleksi item pada transaksi ini (satu transaksi -> banyak item)
        // Inisialisasi agar tidak null saat diakses
        public ICollection<TransactionItem> Items { get; set; } = new List<TransactionItem>();
    }
}
