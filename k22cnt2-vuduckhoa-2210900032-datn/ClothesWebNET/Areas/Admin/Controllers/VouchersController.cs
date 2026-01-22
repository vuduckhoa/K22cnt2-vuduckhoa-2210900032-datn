using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ClothesWebNET.Models;

namespace ClothesWebNET.Areas.Admin.Controllers
{
    public class VouchersController : Controller
    {
        private readonly CLOTHESEntities db = new CLOTHESEntities();

        // GET: Admin/Vouchers
        public ActionResult Index()
        {
            if (Session["SESSION_GROUP_ADMIN"] == null) return Redirect("~/login");
            return View(db.Vouchers.OrderByDescending(v => v.dateEnd).ToList());
        }

        // GET: Admin/Vouchers/Create
        public ActionResult Create()
        {
            if (Session["SESSION_GROUP_ADMIN"] == null) return Redirect("~/login");
            var model = new Voucher
            {
                dateStart = DateTime.Today,
                dateEnd = DateTime.Today.AddDays(7)
            };
            return View(model);
        }

        // POST: Admin/Vouchers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "idVoucher,percent,dateStart,dateEnd")] Voucher voucher)
        {
            if (Session["SESSION_GROUP_ADMIN"] == null) return Redirect("~/login");

            NormalizeVoucher(voucher);
            ValidateVoucher(voucher);

            if (string.IsNullOrWhiteSpace(voucher.idVoucher))
            {
                var count = db.Vouchers.Count() + 1;
                voucher.idVoucher = "VC" + count;
            }

            if (db.Vouchers.Any(v => v.idVoucher == voucher.idVoucher))
            {
                ModelState.AddModelError("idVoucher", "Mã voucher đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                db.Vouchers.Add(voucher);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(voucher);
        }

        // GET: Admin/Vouchers/Edit/VC01
        public ActionResult Edit(string id)
        {
            if (Session["SESSION_GROUP_ADMIN"] == null) return Redirect("~/login");
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var voucher = db.Vouchers.Find(id);
            if (voucher == null) return HttpNotFound();
            return View(voucher);
        }

        // POST: Admin/Vouchers/Edit/VC01
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "idVoucher,percent,dateStart,dateEnd")] Voucher voucher)
        {
            if (Session["SESSION_GROUP_ADMIN"] == null) return Redirect("~/login");

            NormalizeVoucher(voucher);
            ValidateVoucher(voucher);

            if (ModelState.IsValid)
            {
                db.Entry(voucher).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(voucher);
        }

        // GET: Admin/Vouchers/Delete/VC01
        public ActionResult Delete(string id)
        {
            if (Session["SESSION_GROUP_ADMIN"] == null) return Redirect("~/login");
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var voucher = db.Vouchers.Find(id);
            if (voucher == null) return HttpNotFound();
            return View(voucher);
        }

        // POST: Admin/Vouchers/Delete/VC01
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (Session["SESSION_GROUP_ADMIN"] == null) return Redirect("~/login");
            var voucher = db.Vouchers.Find(id);
            if (voucher != null)
            {
                db.Vouchers.Remove(voucher);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        private void NormalizeVoucher(Voucher voucher)
        {
            if (voucher == null) return;
            voucher.idVoucher = (voucher.idVoucher ?? "").Trim();
            voucher.percent = (voucher.percent ?? "").Trim().Replace("%", "");
        }

        private void ValidateVoucher(Voucher voucher)
        {
            if (voucher == null) return;

            if (string.IsNullOrWhiteSpace(voucher.percent))
            {
                ModelState.AddModelError("percent", "Vui lòng nhập phần trăm giảm.");
            }
            else
            {
                int p;
                if (!int.TryParse(voucher.percent, out p) || p <= 0 || p > 100)
                {
                    ModelState.AddModelError("percent", "Phần trăm giảm phải là số từ 1 đến 100.");
                }
            }

            if (voucher.dateEnd < voucher.dateStart)
            {
                ModelState.AddModelError("dateEnd", "Ngày kết thúc phải >= ngày bắt đầu.");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}


