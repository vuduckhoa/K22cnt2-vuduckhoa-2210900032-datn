using ClothesWebNET.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace ClothesWebNET.Controllers
{
    public class FavoriteController : Controller
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

        // GET: /Favorite
        public ActionResult Index()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Redirect("/login");
            }

            try
            {
                // 1) Get favorite product ids (Favorite table is not in EDMX)
                var favSql = @"
SELECT idUser, idProduct, createdAt
FROM Favorite
WHERE idUser = @p0
ORDER BY ISNULL(createdAt, '19000101') DESC";

                var favorites = db.Database.SqlQuery<Favorite>(favSql, userId).ToList();
                var ids = favorites.Select(f => f.idProduct).Distinct().ToList();

                // 2) Load products & images via EF (reliable table mapping via EDMX)
                var products = db.Products
                    .Include("ImageProducts")
                    .Where(p => ids.Contains(p.idProduct))
                    .ToList();

                ViewBag.FavoriteCount = favorites.Count;
                ViewBag.ProductCount = products.Count;
                ViewBag.FavoriteIds = string.Join(", ", ids);

                var list = favorites
                    .Select(f =>
                    {
                        var p = products.FirstOrDefault(x => x.idProduct == f.idProduct);
                        if (p == null) return null;
                        return new FavoriteProductDTO
                        {
                            idProduct = p.idProduct,
                            nameProduct = p.nameProduct,
                            price = p.price,
                            URLImage = p.ImageProducts.FirstOrDefault() != null ? p.ImageProducts.FirstOrDefault().URLImage : null
                        };
                    })
                    .Where(x => x != null)
                    .ToList();

                ViewBag.FavoriteError = null;
                return View(list);
            }
            catch (Exception ex)
            {
                // Show a friendly message instead of a blank UI when DB table/query is missing
                ViewBag.FavoriteError = ex.Message;
                return View(Enumerable.Empty<FavoriteProductDTO>());
            }
        }

        [HttpPost]
        public JsonResult Toggle(string idProduct)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { requiresLogin = true });
            }

            if (string.IsNullOrWhiteSpace(idProduct))
            {
                return Json(new { requiresLogin = false, error = "Missing idProduct" });
            }

            try
            {
                var exists = db.Database
                    .SqlQuery<int>(
                        "SELECT COUNT(1) FROM Favorite WHERE idUser = @p0 AND idProduct = @p1",
                        userId,
                        idProduct
                    )
                    .FirstOrDefault() > 0;

                if (exists)
                {
                    db.Database.ExecuteSqlCommand(
                        "DELETE FROM Favorite WHERE idUser = @p0 AND idProduct = @p1",
                        userId,
                        idProduct
                    );
                    return Json(new { requiresLogin = false, isFavorited = false });
                }

                db.Database.ExecuteSqlCommand(
                    "INSERT INTO Favorite(idUser, idProduct, createdAt) VALUES(@p0, @p1, GETDATE())",
                    userId,
                    idProduct
                );
                return Json(new { requiresLogin = false, isFavorited = true });
            }
            catch (Exception ex)
            {
                return Json(new { requiresLogin = false, error = ex.Message });
            }
        }
    }
}


