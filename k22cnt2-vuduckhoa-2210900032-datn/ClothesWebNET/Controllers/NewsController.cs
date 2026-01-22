using ClothesWebNET.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ClothesWebNET.Controllers
{
    public class NewsController : Controller
    {
        private readonly CLOTHESEntities db = new CLOTHESEntities();

        // GET: /News
        public ActionResult Index()
        {
            try
            {
                var list = db.Database.SqlQuery<Models.News>(
                    "SELECT idNews, title, coverImageUrl, authorName, createdAt, updatedAt FROM News ORDER BY ISNULL(updatedAt, createdAt) DESC"
                ).ToList();
                ViewBag.NewsError = null;
                return View(list);
            }
            catch (Exception ex)
            {
                ViewBag.NewsError = ex.Message;
                return View(new List<Models.News>());
            }
        }

        // GET: /News/Details/{id}
        public ActionResult Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return RedirectToAction("Index");
            try
            {
                var news = db.Database.SqlQuery<Models.News>(
                    "SELECT idNews, title, coverImageUrl, authorName, createdAt, updatedAt FROM News WHERE idNews = @p0",
                    id
                ).FirstOrDefault();

                if (news == null) return RedirectToAction("Index");

                var sections = db.Database.SqlQuery<NewsSection>(
                    "SELECT idSection, idNews, heading, imageUrl, content, sortOrder FROM NewsSection WHERE idNews = @p0 ORDER BY sortOrder ASC",
                    id
                ).ToList();

                return View(new NewsDetailVM { News = news, Sections = sections });
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}


