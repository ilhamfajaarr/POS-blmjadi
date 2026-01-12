using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Models
{
    // Model yang merepresentasikan produk/barang di POS
    public class Product
    {
        // Primary key produk
        public int Id { get; set; }

        // Nama produk (boleh null jika belum diisi)
        public string? Name { get; set; }

        // Harga produk; disimpan sebagai decimal dengan precision di DB
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        // URL atau path gambar produk (relatif/absolut)
        public string? ImageUrl { get; set; }
    }
}
