using ClothesWebNET.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace ClothesWebNET.Areas.Admin.Controllers
{
    public class NewsController : Controller
    {
        private readonly CLOTHESEntities db = new CLOTHESEntities();

        private string SaveNewsImage(System.Web.HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength <= 0) return null;
            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";
            var fileName = Guid.NewGuid().ToString("N") + ext;
            var relDir = "/images/news/";
            var absDir = Server.MapPath(relDir);
            Directory.CreateDirectory(absDir);
            var absPath = Path.Combine(absDir, fileName);
            file.SaveAs(absPath);
            return relDir + fileName;
        }

        // GET: Admin/News
        public ActionResult Index()
        {
            if (Session["SESSION_GROUP_ADMIN"] == null) return Redirect("~/login");
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

        // GET: Admin/News/Create
        public ActionResult Create()
        {
            if (Session["SESSION_GROUP_ADMIN"] == null) return Redirect("~/login");
            ViewBag.Sections = new List<NewsSection>();
            return View();
        }

        // POST: Admin/News/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string title, string authorName, string[] sectionHeading, string[] sectionContent, System.Web.HttpPostedFileBase coverImage, System.Web.HttpPostedFileBase[] sectionImages)
        {
            if (Session["SESSION_GROUP_ADMIN"] == null) return Redirect("~/login");

            title = (title ?? "").Trim();
            authorName = (authorName ?? "").Trim();

            if (string.IsNullOrWhiteSpace(title))
            {
                ModelState.AddModelError("title", "Vui lòng nhập tựa đề.");
            }

            var sections = BuildSections(sectionHeading, sectionContent);
            if (sections.Count == 0)
            {
                ModelState.AddModelError("sections", "Vui lòng thêm ít nhất 1 mục lớn.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Sections = sections;
                return View();
            }

            var idNews = Guid.NewGuid().ToString();
            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    var coverUrl = SaveNewsImage(coverImage);
                    db.Database.ExecuteSqlCommand(
                        "INSERT INTO News(idNews, title, coverImageUrl, authorName, createdAt) VALUES(@p0, @p1, @p2, @p3, GETDATE())",
                        idNews, title, coverUrl, string.IsNullOrWhiteSpace(authorName) ? null : authorName
                    );

                    for (var i = 0; i < sections.Count; i++)
                    {
                        var s = sections[i];
                        var imgUrl = (sectionImages != null && i < sectionImages.Length) ? SaveNewsImage(sectionImages[i]) : null;
                        db.Database.ExecuteSqlCommand(
                            "INSERT INTO NewsSection(idSection, idNews, heading, imageUrl, content, sortOrder) VALUES(@p0, @p1, @p2, @p3, @p4, @p5)",
                            Guid.NewGuid().ToString(), idNews, s.heading, imgUrl, s.content, s.sortOrder
                        );
                    }

                    tx.Commit();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    ViewBag.NewsError = ex.Message;
                    ViewBag.Sections = sections;
                    return View();
                }
            }
        }

        // GET: Admin/News/Edit/{id}
        public ActionResult Edit(string id)
        {
            if (Session["SESSION_GROUP_ADMIN"] == null) return Redirect("~/login");
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

                ViewBag.News = news;
                ViewBag.Sections = sections;
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.NewsError = ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Admin/News/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string idNews, string title, string authorName, string coverImageUrl, string[] sectionHeading, string[] sectionContent, string[] sectionImageUrl, System.Web.HttpPostedFileBase coverImage, System.Web.HttpPostedFileBase[] sectionImages)
        {
            if (Session["SESSION_GROUP_ADMIN"] == null) return Redirect("~/login");

            idNews = (idNews ?? "").Trim();
            title = (title ?? "").Trim();
            authorName = (authorName ?? "").Trim();

            if (string.IsNullOrWhiteSpace(idNews)) return RedirectToAction("Index");
            if (string.IsNullOrWhiteSpace(title))
            {
                ModelState.AddModelError("title", "Vui lòng nhập tựa đề.");
            }

            var sections = BuildSections(sectionHeading, sectionContent);
            if (sections.Count == 0)
            {
                ModelState.AddModelError("sections", "Vui lòng thêm ít nhất 1 mục lớn.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.News = new Models.News { idNews = idNews, title = title, authorName = authorName };
                ViewBag.Sections = sections;
                return View();
            }

            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    var newCover = SaveNewsImage(coverImage);
                    var finalCover = !string.IsNullOrWhiteSpace(newCover) ? newCover : (coverImageUrl ?? "").Trim();
                    db.Database.ExecuteSqlCommand(
                        "UPDATE News SET title=@p1, coverImageUrl=@p2, authorName=@p3, updatedAt=GETDATE() WHERE idNews=@p0",
                        idNews, title, string.IsNullOrWhiteSpace(finalCover) ? null : finalCover, string.IsNullOrWhiteSpace(authorName) ? null : authorName
                    );

                    db.Database.ExecuteSqlCommand("DELETE FROM NewsSection WHERE idNews=@p0", idNews);

                    for (var i = 0; i < sections.Count; i++)
                    {
                        var s = sections[i];
                        var keepImg = (sectionImageUrl != null && i < sectionImageUrl.Length) ? (sectionImageUrl[i] ?? "").Trim() : "";
                        var newImg = (sectionImages != null && i < sectionImages.Length) ? SaveNewsImage(sectionImages[i]) : null;
                        var finalImg = !string.IsNullOrWhiteSpace(newImg) ? newImg : keepImg;
                        db.Database.ExecuteSqlCommand(
                            "INSERT INTO NewsSection(idSection, idNews, heading, imageUrl, content, sortOrder) VALUES(@p0, @p1, @p2, @p3, @p4, @p5)",
                            Guid.NewGuid().ToString(), idNews, s.heading, string.IsNullOrWhiteSpace(finalImg) ? null : finalImg, s.content, s.sortOrder
                        );
                    }

                    tx.Commit();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    ViewBag.NewsError = ex.Message;
                    ViewBag.News = new Models.News { idNews = idNews, title = title, authorName = authorName, coverImageUrl = coverImageUrl };
                    ViewBag.Sections = sections;
                    return View();
                }
            }
        }

        // GET: Admin/News/Delete/{id}
        public ActionResult Delete(string id)
        {
            if (Session["SESSION_GROUP_ADMIN"] == null) return Redirect("~/login");
            if (string.IsNullOrWhiteSpace(id)) return RedirectToAction("Index");

            try
            {
                var news = db.Database.SqlQuery<Models.News>(
                    "SELECT idNews, title, coverImageUrl, authorName, createdAt, updatedAt FROM News WHERE idNews = @p0",
                    id
                ).FirstOrDefault();
                if (news == null) return RedirectToAction("Index");
                return View(news);
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }

        // POST: Admin/News/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string idNews)
        {
            if (Session["SESSION_GROUP_ADMIN"] == null) return Redirect("~/login");
            if (string.IsNullOrWhiteSpace(idNews)) return RedirectToAction("Index");

            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    db.Database.ExecuteSqlCommand("DELETE FROM NewsSection WHERE idNews=@p0", idNews);
                    db.Database.ExecuteSqlCommand("DELETE FROM News WHERE idNews=@p0", idNews);
                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                }
            }
            return RedirectToAction("Index");
        }

        private List<NewsSection> BuildSections(string[] headings, string[] contents)
        {
            var list = new List<NewsSection>();
            var len = Math.Max(headings != null ? headings.Length : 0, contents != null ? contents.Length : 0);
            for (var i = 0; i < len; i++)
            {
                var h = headings != null && i < headings.Length ? (headings[i] ?? "").Trim() : "";
                var c = contents != null && i < contents.Length ? (contents[i] ?? "").Trim() : "";
                if (string.IsNullOrWhiteSpace(h) && string.IsNullOrWhiteSpace(c)) continue;
                list.Add(new NewsSection { heading = h, content = c, sortOrder = list.Count });
            }
            return list;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}


