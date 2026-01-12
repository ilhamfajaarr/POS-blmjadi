namespace POS.Models
{
    // Model user untuk otentikasi/otorisasi sederhana
    public class User
    {
        // Primary key user
        public int Id { get; set; }

        // Nama pengguna/login
        public string Username { get; set; }

        // Kata sandi
        public string Password { get; set; }

        // Peran/role user (mis. "Admin", "Kasir")
        public string Role { get; set; }
    }
}
