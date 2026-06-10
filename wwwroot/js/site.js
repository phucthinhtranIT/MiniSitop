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

async function capNhatSoLuongGio() {
    const dem = document.getElementById("cartCount");
    if (!dem) return;

    try {
        const response = await fetch("/GioHang/SoLuong", {
            headers: { "Accept": "application/json" }
        });
        const data = await response.json();
        const soLuong = Number(data.soLuong || 0);

        dem.textContent = soLuong;
        dem.classList.toggle("hidden", soLuong === 0);
    } catch {
        dem.classList.add("hidden");
    }
}

async function themSanPhamVaoGio(sanPhamId) {
    const body = new URLSearchParams();
    body.append("sanPhamId", sanPhamId);

    const response = await fetch("/GioHang/Them", {
        method: "POST",
        headers: {
            "Content-Type": "application/x-www-form-urlencoded",
            "Accept": "application/json"
        },
        body
    });

    return response.json();
}

function ganNutThemGioHang() {
    document.querySelectorAll(".cart-add-button").forEach((nut) => {
        nut.addEventListener("click", async () => {
            try {
                const ketQua = await themSanPhamVaoGio(nut.dataset.id);
                hienThiThongBaoGioHang(ketQua.thongBao || "Đã cập nhật giỏ hàng.");

                if (ketQua.canDangNhap) {
                    window.setTimeout(() => {
                        window.location.href = "/DangNhap";
                    }, 900);
                    return;
                }

                await capNhatSoLuongGio();
            } catch {
                hienThiThongBaoGioHang("Không thể cập nhật giỏ hàng. Vui lòng thử lại.");
            }
        });
    });
}

function ganMenuTaiKhoan() {
    const khung = document.querySelector("[data-account-menu]");
    if (!khung) return;

    const nut = khung.querySelector("[data-account-menu-button]");
    const menu = khung.querySelector("[data-account-menu-panel]");
    if (!nut || !menu) return;

    const dongMenu = () => {
        menu.classList.add("invisible", "opacity-0");
        nut.setAttribute("aria-expanded", "false");
    };

    const moMenu = () => {
        menu.classList.remove("invisible", "opacity-0");
        nut.setAttribute("aria-expanded", "true");
    };

    nut.addEventListener("click", (event) => {
        event.stopPropagation();
        const dangMo = nut.getAttribute("aria-expanded") === "true";
        if (dangMo) {
            dongMenu();
        } else {
            moMenu();
        }
    });

    document.addEventListener("click", (event) => {
        if (!khung.contains(event.target)) {
            dongMenu();
        }
    });

    document.addEventListener("keydown", (event) => {
        if (event.key === "Escape") {
            dongMenu();
        }
    });
}

document.addEventListener("DOMContentLoaded", () => {
    capNhatSoLuongGio();
    ganNutThemGioHang();
    ganMenuTaiKhoan();
});
