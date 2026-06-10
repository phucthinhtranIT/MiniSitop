const CART_KEY = "webqlministop_cart";

function docTienVietNam(giaTri) {
    return new Intl.NumberFormat("vi-VN", {
        style: "currency",
        currency: "VND",
        maximumFractionDigits: 0
    }).format(Number(giaTri || 0));
}

function thoatHtml(giaTri) {
    const div = document.createElement("div");
    div.textContent = giaTri || "";
    return div.innerHTML;
}

function layGioHang() {
    try {
        return JSON.parse(localStorage.getItem(CART_KEY)) || [];
    } catch {
        return [];
    }
}

function luuGioHang(gioHang) {
    localStorage.setItem(CART_KEY, JSON.stringify(gioHang));
    capNhatSoLuongGio();
}

function capNhatSoLuongGio() {
    const dem = document.getElementById("cartCount");
    if (!dem) return;

    const tongSoLuong = layGioHang().reduce((tong, sanPham) => tong + sanPham.soLuong, 0);
    dem.textContent = tongSoLuong;
    dem.classList.toggle("hidden", tongSoLuong === 0);
}

function themSanPhamVaoGio(duLieu) {
    const gioHang = layGioHang();
    const sanPhamHienTai = gioHang.find((sanPham) => sanPham.id === duLieu.id);
    const tonKho = Number(duLieu.tonKho || 999);

    if (sanPhamHienTai) {
        if (sanPhamHienTai.soLuong >= tonKho) {
            return { thanhCong: false, thongBao: "Số lượng trong giỏ đã bằng tồn kho hiện có." };
        }

        sanPhamHienTai.soLuong += 1;
    } else {
        gioHang.push({
            id: duLieu.id,
            ma: duLieu.ma,
            ten: duLieu.ten,
            gia: Number(duLieu.gia || 0),
            hinhAnh: duLieu.hinhAnh,
            donVi: duLieu.donVi,
            tonKho,
            soLuong: 1
        });
    }

    luuGioHang(gioHang);
    return { thanhCong: true, thongBao: "Đã thêm sản phẩm vào giỏ hàng." };
}

function hienThiThongBaoGioHang(noiDung) {
    let thongBao = document.getElementById("cartToast");
    if (!thongBao) {
        thongBao = document.createElement("div");
        thongBao.id = "cartToast";
        thongBao.className = "fixed bottom-5 right-5 z-[80] rounded-xl bg-slate-900 px-4 py-3 text-sm font-semibold text-white shadow-xl transition-opacity";
        document.body.appendChild(thongBao);
    }

    thongBao.textContent = noiDung;
    thongBao.classList.remove("opacity-0");

    window.clearTimeout(thongBao.dataset.timer);
    thongBao.dataset.timer = window.setTimeout(() => {
        thongBao.classList.add("opacity-0");
    }, 1800);
}

function ganNutThemGioHang() {
    document.querySelectorAll(".cart-add-button").forEach((nut) => {
        nut.addEventListener("click", () => {
            const ketQua = themSanPhamVaoGio({
                id: nut.dataset.id,
                ma: nut.dataset.ma,
                ten: nut.dataset.ten,
                gia: nut.dataset.gia,
                hinhAnh: nut.dataset.hinh,
                donVi: nut.dataset.donvi,
                tonKho: nut.dataset.tonkho
            });

            hienThiThongBaoGioHang(ketQua.thongBao);
        });
    });
}

function capNhatSoLuongSanPham(id, soLuongMoi) {
    const gioHang = layGioHang();
    const sanPham = gioHang.find((item) => item.id === id);
    if (!sanPham) return;

    sanPham.soLuong = Math.max(1, Math.min(Number(soLuongMoi || 1), sanPham.tonKho || 999));
    luuGioHang(gioHang);
    veTrangGioHang();
}

function xoaSanPhamKhoiGio(id) {
    luuGioHang(layGioHang().filter((sanPham) => sanPham.id !== id));
    veTrangGioHang();
}

function xoaTatCaGioHang() {
    luuGioHang([]);
    veTrangGioHang();
}

function veTrangGioHang() {
    const khung = document.getElementById("cartItems");
    const tongTienEl = document.getElementById("cartTotal");
    const trangTrong = document.getElementById("emptyCart");
    const noiDungGio = document.getElementById("cartContent");
    if (!khung || !tongTienEl || !trangTrong || !noiDungGio) return;

    const gioHang = layGioHang();
    khung.innerHTML = "";

    trangTrong.classList.toggle("hidden", gioHang.length > 0);
    noiDungGio.classList.toggle("hidden", gioHang.length === 0);

    let tongTien = 0;
    gioHang.forEach((sanPham) => {
        const thanhTien = sanPham.gia * sanPham.soLuong;
        tongTien += thanhTien;

        const dong = document.createElement("article");
        dong.className = "grid grid-cols-[84px_1fr] gap-4 rounded-xl border border-slate-100 bg-white p-3 shadow-sm md:grid-cols-[96px_1fr_auto]";
        dong.innerHTML = `
            <img src="${thoatHtml(sanPham.hinhAnh)}" alt="${thoatHtml(sanPham.ten)}" class="h-20 w-20 rounded-lg object-cover md:h-24 md:w-24" />
            <div class="min-w-0">
                <div class="text-xs text-slate-400">${thoatHtml(sanPham.ma)}</div>
                <h2 class="mt-1 line-clamp-2 font-bold text-slate-900">${thoatHtml(sanPham.ten)}</h2>
                <div class="mt-1 text-sm text-slate-500">${docTienVietNam(sanPham.gia)} / ${thoatHtml(sanPham.donVi || "sp")}</div>
                <div class="mt-3 flex items-center gap-2">
                    <button type="button" class="cart-qty h-8 w-8 rounded-full border border-slate-200 text-slate-600 hover:border-brand-400 hover:text-brand-600" data-id="${sanPham.id}" data-qty="${sanPham.soLuong - 1}">-</button>
                    <input class="cart-input h-8 w-14 rounded-lg border border-slate-200 text-center text-sm font-semibold" value="${sanPham.soLuong}" min="1" max="${sanPham.tonKho}" data-id="${sanPham.id}" />
                    <button type="button" class="cart-qty h-8 w-8 rounded-full border border-slate-200 text-slate-600 hover:border-brand-400 hover:text-brand-600" data-id="${sanPham.id}" data-qty="${sanPham.soLuong + 1}">+</button>
                    <span class="text-xs text-slate-400">Tối đa ${sanPham.tonKho}</span>
                </div>
            </div>
            <div class="col-span-2 flex items-center justify-between border-t border-slate-100 pt-3 md:col-span-1 md:block md:border-t-0 md:pt-0 md:text-right">
                <div class="font-bold text-brand-600">${docTienVietNam(thanhTien)}</div>
                <button type="button" class="cart-remove mt-0 text-sm font-semibold text-red-500 hover:text-red-600 md:mt-4" data-id="${sanPham.id}">Xóa</button>
            </div>
        `;
        khung.appendChild(dong);
    });

    tongTienEl.textContent = docTienVietNam(tongTien);

    document.querySelectorAll(".cart-qty").forEach((nut) => {
        nut.addEventListener("click", () => capNhatSoLuongSanPham(nut.dataset.id, nut.dataset.qty));
    });
    document.querySelectorAll(".cart-input").forEach((input) => {
        input.addEventListener("change", () => capNhatSoLuongSanPham(input.dataset.id, input.value));
    });
    document.querySelectorAll(".cart-remove").forEach((nut) => {
        nut.addEventListener("click", () => xoaSanPhamKhoiGio(nut.dataset.id));
    });
}

document.addEventListener("DOMContentLoaded", () => {
    capNhatSoLuongGio();
    ganNutThemGioHang();
    veTrangGioHang();

    const nutXoaHet = document.getElementById("clearCart");
    if (nutXoaHet) {
        nutXoaHet.addEventListener("click", xoaTatCaGioHang);
    }

    const nutDatHang = document.getElementById("checkoutCart");
    if (nutDatHang) {
        nutDatHang.addEventListener("click", () => {
            if (layGioHang().length === 0) {
                hienThiThongBaoGioHang("Giỏ hàng đang trống.");
                return;
            }

            hienThiThongBaoGioHang("Đã ghi nhận giỏ hàng. Bước tạo đơn hàng sẽ nối với DonHang sau.");
        });
    }
});
