let total = 0;
let cart = [];

// TANGKAP KLIK TOMBOL "ADD"
// Pakai event delegation supaya semua tombol .add-btn bisa diproses
document.addEventListener('click', function (e) {
    const btn = e.target.closest('.add-btn');
    if (!btn) return;

    // Ambil nama dan harga dari atribut HTML
    const name = btn.getAttribute('data-name');
    const price = parseFloat(btn.getAttribute('data-price')) || 0;

    addToCart(name, price);
});


// TAMBAH PRODUK KE KERANJANG

function addToCart(name, price) {
    // Cek apakah produk sudah ada di keranjang
    const existing = cart.find(p => p.name === name);

    if (existing) {
        // Kalau sudah ada, tinggal tambah qty
        existing.qty++;
    } else {
        // Kalau belum ada, tambahkan sebagai item baru
        cart.push({ name, price, qty: 1 });
    }

    // Update tampilan keranjang
    updateCart();
}

// KURANGI JUMLAH ITEM

function decreaseQty(index) {
    if (cart[index].qty > 1) {
        // Kalau masih lebih dari 1, kurangi saja
        cart[index].qty--;
    } else {
        // Kalau tinggal 1, hapus item dari keranjang
        cart.splice(index, 1);
    }

    updateCart();
}


// UPDATE TAMPILAN KERANJANG

function updateCart() {
    const cartBody = document.getElementById("cartBody");
    cartBody.innerHTML = "";
    total = 0;

    // Loop semua item di keranjang
    cart.forEach((item, index) => {
        total += item.price * item.qty;

        // Buat baris tabel untuk tiap item
        const row = `
            <tr>
                <td>${escapeHtml(item.name)}</td>
                <td>
                    <button class="btn btn-sm btn-danger me-1" onclick="decreaseQty(${index})">-</button>
                    ${item.qty}
                    <button class="btn btn-sm btn-success ms-1" onclick="addToCart('${jsEscape(item.name)}', ${item.price})">+</button>
                </td>
                <td>Rp ${(item.price * item.qty).toLocaleString()}</td>
            </tr>`;
        cartBody.innerHTML += row;
    });

    // Hitung pajak dan total akhir
    const tax = total * 0.1;
    const grandTotal = total + tax;

    // Tampilkan ke UI
    document.getElementById("subtotal").textContent = total.toLocaleString();
    document.getElementById("taxAmount").textContent = tax.toLocaleString();
    document.getElementById("totalAmount").textContent = grandTotal.toLocaleString();
}

// SIMPAN TRANSAKSI KE BACKEND
// type: "print" atau "whatsapp"

async function saveTransaction(type) {
    if (cart.length === 0) {
        alert("Keranjang masih kosong!");
        return;
    }

    // Ambil nomor WhatsApp dari input (kalau ada)
    let waNumber = document.getElementById("waNumber")?.value || "";

    // Kalau mau kirim WhatsApp tapi nomor kosong
    if (type === "whatsapp" && waNumber.trim() === "") {
        alert("Masukkan nomor WhatsApp terlebih dahulu!");
        return;
    }

    // Ubah format 08xxx -> 628xxx
    if (waNumber.startsWith("08")) {
        waNumber = "62" + waNumber.substring(1);
    }

    // Bentuk data JSON yang akan dikirim ke server
    const request = {
        TotalAmount: total,
        WhatsAppNumber: waNumber,
        Items: cart.map(item => ({
            ProductName: item.name,
            Quantity: item.qty,
            Price: item.price
        }))
    };

    console.log("Kirim JSON:", JSON.stringify(request, null, 2));

    // Kirim data ke Razor Page (handler SaveTransaction)
    const response = await fetch("/Index?handler=SaveTransaction", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(request)
    });

    if (response.ok) {
        // Kalau pilih cetak
        if (type === "print") {
            printReceipt();
        }
        // Kalau pilih kirim WhatsApp
        else if (type === "whatsapp") {
            alert("Struk berhasil dikirim ke WhatsApp!");
        }

        // Reset keranjang setelah transaksi berhasil
        cart = [];
        updateCart();

        // Kosongkan input WA
        if (document.getElementById("waNumber")) {
            document.getElementById("waNumber").value = "";
        }
    }
    else {
        alert("Gagal menyimpan transaksi.");
    }
}


// CETAK STRUK

function printReceipt() {
    const printArea = document.getElementById('printArea');
    if (!printArea) {
        alert('Tidak ada struk untuk dicetak.');
        return;
    }

    window.print();
}


// ESCAPE STRING UNTUK JAVASCRIPT
// Supaya aman saat dipakai di onclick

function jsEscape(s) {
    return String(s).replace(/'/g, "\\'").replace(/"/g, '\\"');
}

// ESCAPE HTML
// Supaya mencegah XSS / karakter aneh di tabel

function escapeHtml(s) {
    return String(s)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

// JAM REAL-TIME DI HALAMAN
function updateClock() {
    const now = new Date();
    document.getElementById("clock").textContent = now.toLocaleTimeString();
}

// Update jam tiap 1 detik
setInterval(updateClock, 1000);
updateClock();
