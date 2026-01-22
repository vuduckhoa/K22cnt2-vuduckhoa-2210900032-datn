using System;
using System.Configuration;
using System.Web.Mvc;

namespace ClothesWebNET.Controllers
{
    public class PaymentController : Controller
    {
        /// <summary>
        /// Get QR code URL for bank transfer payment (sử dụng ảnh QR thật)
        /// </summary>
        /// <param name="amount">Amount to pay (in VND, without thousand separator)</param>
        /// <param name="content">Payment content/note</param>
        /// <returns>JSON with QR code image URL</returns>
        [HttpGet]
        public JsonResult GenerateQRCode(decimal amount, string content = "")
        {
            // Đọc thông tin tài khoản ngân hàng từ Web.config
            var bankAccount = ConfigurationManager.AppSettings["BankAccount"] ?? "270820004";
            var bankName = ConfigurationManager.AppSettings["BankName"] ?? "MBBank";
            var accountName = ConfigurationManager.AppSettings["BankAccountName"] ?? "DucK OFFICIAL STORE";

            // Format nội dung chuyển khoản
            var paymentContent = string.IsNullOrWhiteSpace(content) 
                ? $"Thanh toan don hang {DateTime.Now:yyyyMMddHHmmss}" 
                : content;

            // Sử dụng ảnh QR thật từ thư mục IMAGES/QR/khoa.jpg
            var qrCodeUrl = Url.Content("~/IMAGES/QR/khoa.jpg");

            return Json(new
            {
                success = true,
                qrCodeUrl = qrCodeUrl,
                bankAccount = bankAccount,
                bankName = bankName,
                accountName = accountName,
                amount = amount,
                content = paymentContent
            }, JsonRequestBehavior.AllowGet);
        }
    }
}

