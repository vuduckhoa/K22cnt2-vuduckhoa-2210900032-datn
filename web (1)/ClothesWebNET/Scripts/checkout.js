
const fullname = document.getElementById('fullname');
const email = document.getElementById('email');
const phone = document.getElementById('phone');
const address = document.getElementById('address');

const city = document.getElementById('province'); //thanh pho
const district = document.getElementById('district'); //quan/huyen
const ward = document.getElementById('village');//

fetch('/data.json')
    .then((response) => response.json())
    .then((data) => renderCity(data));

function renderCity(data) {
    for (var item of data) {
        // kh?i t?o ra ??i t??ng c?c t?nh th?nh ph?
        city.options[city.options.length] = new Option(item.Name, item.Id);
    }

    // x? l? khi thay ??i t?nh th?nh th? s? hi?n th? ra qu?n huy?n thu?c t?nh th?nh ??
    city.onchange = () => {
        district.length = 1;

        console.log(city.value);
        // ki?m tra gi? tr? value xem c? r?ng ko l? none th? ko th?c hi?n render c?c qu?n ra
        if (city.value != '') {
            // l?c ra d? li?u khi ng??i d?ng tr? v?o t?nh th?nh ph?
            const result = data.filter((n) => n.Id === city.value);
            // nguy?n nh?n result[0].District
            // gi?i th?ch :
            // th? l?c ta l?c d? li?u result xong th? k?t qu? n? s? tr? cho ra m?t m?ng
            // trong m?ng ?? ch?a ??i t??ng [{}]
            // th? c? ph?i ??i t??ng m?nh g?i trong ?? ?ang ? index = 0 th? m?nh ph?i g?i n? ra
            // l?
            //   result[0] th? l?c n?y n? ra  object{} th? trong object m?nh g?i ??n attribute l? DISTRICTS
            //     => result[0].Districts
            for (var item of result[0].Districts) {
                district.options[district.options.length] = new Option(
                    item.Name,
                    item.Id
                );
            }
        } else {
            // do nothing
        }
    };

    district.onchange = () => {
        ward.length = 1;
        const result = data.filter((el) => el.Id === city.value);
        if (district.value != ' ') {
            // l?y d? li?u qu?n v? trong d? li?u qu?n ch? t?n ???ng
            const resultDistrict = result[0].Districts.filter(
                (el) => el.Id === district.value
            );

            for (var item of resultDistrict[0].Wards) {
                ward.options[ward.options.length] = new Option(item.Name, item.id);
            }
        }
    };
}
// renderCity(dt);
import validation from './validation.js';

//submit form
const listCart = JSON.parse(window.localStorage.getItem('cart'));
if (!listCart || listCart.length==0) {

    window.location.href='/'
}

// voucher state (total is in "thousand" units like existing code)
let appliedVoucherId = null;
let appliedDiscount = 0;
let appliedNewTotal = null;

function formatMoneyThousand(n) {
    if (typeof n !== 'number') n = Number(n || 0);
    return n.toLocaleString() + ",000VND";
}

function renderVoucherList(items) {
    const box = document.getElementById('voucherList');
    if (!box) return;
    if (!items || items.length === 0) {
        box.innerHTML = '';
        return;
    }

    function parseDotNetDate(value) {
        if (!value) return null;
        // ASP.NET often serializes DateTime as "/Date(1700000000000)/"
        if (typeof value === 'string') {
            const m = /\/Date\((\-?\d+)\)\//.exec(value);
            if (m) return new Date(parseInt(m[1], 10));
        }
        // If it's already a valid date string or number
        const d = new Date(value);
        return isNaN(d.getTime()) ? null : d;
    }

    function formatDateVN(d) {
        if (!d) return '';
        return d.toLocaleDateString('vi-VN');
    }

    const html = items.map(v => {
        const percent = (v.percent || '').toString().replace('%', '');
        const endDate = parseDotNetDate(v.dateEnd);
        return `
            <div style="display:flex; align-items:center; justify-content:space-between; padding:8px 10px; border:1px solid rgba(0,0,0,0.08); border-radius:8px; margin-bottom:8px;">
                <div>
                    <div style="font-weight:600;">${v.idVoucher}</div>
                    <div style="font-size:12px; color:#6b7280;">Giảm ${percent}% (đến ${formatDateVN(endDate)})</div>
                </div>
                <button type="button" class="btnVoucher" data-code="${v.idVoucher}" style="padding:6px 10px;">Chọn</button>
            </div>
        `;
    }).join('');

    box.innerHTML = html;
}

function updateTotalsUI(baseTotal) {
    const totalMoney = document.getElementById('totalMoney');
    const tamTinh = document.getElementById('tamTinh');
    if (!totalMoney || !tamTinh) return;

    const totalToShow = (appliedNewTotal !== null) ? appliedNewTotal : baseTotal;
    totalMoney.innerHTML = formatMoneyThousand(totalToShow);
    tamTinh.innerHTML = formatMoneyThousand(baseTotal);

    const row = document.getElementById('voucherRow');
    const discountEl = document.getElementById('voucherDiscount');
    if (row && discountEl) {
        if (appliedDiscount > 0) {
            row.style.display = '';
            discountEl.innerHTML = '-' + formatMoneyThousand(appliedDiscount);
        } else {
            row.style.display = 'none';
            discountEl.innerHTML = formatMoneyThousand(0);
        }
    }
}

function calcBaseTotal() {
    let total = 0;
    listCart.forEach((el) => {
        let converNumberAmount = Number(el.amount);
        let intoMoney = el.price * converNumberAmount;
        total += intoMoney;
    });
    return total;
}

// Initialize totals UI once (base total)
updateTotalsUI(calcBaseTotal());

// Load available vouchers at checkout
fetch('/Voucher/Available')
    .then(r => r.json())
    .then(res => renderVoucherList(res.items))
    .catch(() => { /* ignore */ });

// Choose voucher from list -> fill input
$(document).on('click', '#voucherList [data-code]', function () {
    const code = $(this).attr('data-code');
    $('#voucherCode').val(code);
});

// Apply voucher
$('#applyVoucher').click(() => {
    const code = ($('#voucherCode').val() || '').trim();
    const baseTotal = calcBaseTotal();
    fetch('/Voucher/Apply', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
        body: new URLSearchParams({ idVoucher: code, total: baseTotal }).toString(),
        credentials: 'same-origin'
    })
        .then(r => r.json())
        .then(res => {
            if (!res || !res.ok) {
                alert(res && res.message ? res.message : 'Không áp dụng được voucher.');
                appliedVoucherId = null;
                appliedDiscount = 0;
                appliedNewTotal = null;
                updateTotalsUI(baseTotal);
                return;
            }

            appliedVoucherId = res.idVoucher;
            appliedDiscount = Number(res.discount || 0);
            appliedNewTotal = Number(res.newTotal || baseTotal);
            updateTotalsUI(baseTotal);
        })
        .catch(() => {
            alert('Không áp dụng được voucher. Vui lòng thử lại.');
        });
});

function uuidv4() {
   
    return ([1e7] + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, c =>
        (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
    );
}
function GenerateId() {
    const d = new Date();
    let ms =d.getTime();
    return ms
}


$('#save_btn').click(() => {
    let checkEmpty = validation.checkRequired([fullname, email, phone, address]);
    let checkEmailInvalid = validation.checkEmail(email);
    let checkPhoneInvalid = validation.checkNumberPhone(phone);
    let checkAddressInvalid = validation.checkAddress([city, district, ward]);

    if (
        checkEmpty &&
        checkEmailInvalid &&
        checkPhoneInvalid &&
        !checkAddressInvalid
    ) {
        var dsChiTietDH = []
        const idBill = "DH" +GenerateId();
        let total = 0;
        let totalQty = 0;
        listCart.forEach((el) => {
            let idDetail = uuidv4();
            let converNumberAmount = Number(el.amount);
            let intoMoney = el.price * converNumberAmount;
            totalQty += Number(el.amount);
            total += intoMoney
            var ctdh = {
                idDetailBill: idDetail, idProduct: el.idFood, idBill: idBill, qty: converNumberAmount, intoMoney: intoMoney
            }
            dsChiTietDH.push(ctdh);
        })

        const idUser = null;
        const customData = {
            idBill: idBill,
            idUser: idUser !== null ? idUser : null,
            Shipping: 50,
            Total: total,
            PTTT: "Tien Mat",
            status: 0,
            detailBill: dsChiTietDH
        }

        let thanhpho = $("#province option:selected").text()
        let quan = $("#village option:selected").text()
        let phuong = $("#district option:selected").text()
        let diachi = address.value + ', ' + quan + ', ' + phuong + ', ' + thanhpho
        let dienthoai = Number(phone.value)
        /* $.post('/Bill/PostBill',"hello", function (res) {
             alert(res);
         })*/

        $.ajax('/Bill/PostBill', {
            data: {
                idBill: idBill,
                idUser: idUser !== null ? idUser : null,
                Shipping: 50,
                Total: (appliedNewTotal !== null ? appliedNewTotal : total),
                totalQty: totalQty,
                nameBook: fullname.value,
                email: email.value,
                phone: dienthoai,
                address: diachi,
                PTTT: "Tien Mat",
                detailBill: dsChiTietDH,
                status: false,
                idVoucher: appliedVoucherId,
            },
            dataType: 'json',
            method: 'Post',
            success: function (res) {
                alert(res);
                window.localStorage.removeItem('cart');
                window.location.replace('/home')
            }
        })
    }
})
