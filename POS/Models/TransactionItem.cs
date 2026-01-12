namespace POS.Models
{
    // Item detail pada sebuah transaksi (baris struk)
    public class TransactionItem
    {
        // Primary key item
        public int Id { get; set; }

        // Nama produk yang dibeli (disimpan sebagai string agar histori tetap)
        public string ProductName { get; set; }

        // Jumlah unit yang dibeli
        public int Quantity { get; set; }

        // Harga per unit pada saat transaksi
        public decimal Price { get; set; }

        // Relasi ke transaksi
        // Foreign key ke tabel Transaction
        public int TransactionId { get; set; }

        // Navigasi ke entitas Transaction
        public Transaction Transaction { get; set; }
    }
}
