using ClothesWebNET.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace ClothesWebNET.Controllers
{
    public class VoucherController : Controller
    {
        private readonly CLOTHESEntities db = new CLOTHESEntities();

        private string GetCurrentUserId()
        {
            if (Session["USER_SESSION"] == null)
            {
                return null;
            }

            var cookie = Request.Cookies["user"];
            if (cookie == null || string.IsNullOrWhiteSpace(cookie.Value))
            {
                return null;
            }

            return cookie.Value;
        }

        [HttpGet]
        public JsonResult Available()
        {
            // Cho phép tất cả người dùng (kể cả chưa đăng nhập) xem danh sách voucher
            var now = DateTime.Now;
            var list = db.Vouchers
                .Where(v => v.dateStart <= now && v.dateEnd >= now)
                .Select(v => new
                {
                    idVoucher = v.idVoucher,
                    percent = v.percent,
                    dateStart = v.dateStart,
                    dateEnd = v.dateEnd
                })
                .ToList();

            return Json(new { items = list }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Apply(string idVoucher, int total)
        {
            // Kiểm tra đăng nhập trước
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { ok = false, requiresLogin = true, message = "Bạn cần đăng nhập để sử dụng voucher." });
            }

            if (string.IsNullOrWhiteSpace(idVoucher))
            {
                return Json(new { ok = false, message = "Vui lòng nhập mã voucher." });
            }

            var now = DateTime.Now;
            var voucher = db.Vouchers.FirstOrDefault(v => v.idVoucher == idVoucher);
            if (voucher == null)
            {
                return Json(new { ok = false, message = "Voucher không tồn tại." });
            }

            if (voucher.dateStart > now || voucher.dateEnd < now)
            {
                return Json(new { ok = false, message = "Voucher đã hết hạn hoặc chưa đến ngày áp dụng." });
            }

            int percent;
            var percentRaw = (voucher.percent ?? "").Trim().Replace("%", "");
            if (!int.TryParse(percentRaw, out percent) || percent <= 0 || percent > 100)
            {
                return Json(new { ok = false, message = "Voucher không hợp lệ (percent)." });
            }

            if (total < 0) total = 0;
            var discount = (int)Math.Floor(total * (percent / 100.0));
            var newTotal = total - discount;

            return Json(new
            {
                ok = true,
                idVoucher = voucher.idVoucher,
                percent = percent,
                discount = discount,
                newTotal = newTotal
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}


