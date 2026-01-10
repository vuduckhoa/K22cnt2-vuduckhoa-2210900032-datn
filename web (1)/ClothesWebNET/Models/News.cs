using System;
using System.Collections.Generic;

namespace ClothesWebNET.Models
{
    // Database-first (EDMX). These models are used for SQL projection only.
    public class News
    {
        public string idNews { get; set; }
        public string title { get; set; }
        public string coverImageUrl { get; set; }
        public string authorName { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime? updatedAt { get; set; }
    }

    public class NewsSection
    {
        public string idSection { get; set; }
        public string idNews { get; set; }
        public string heading { get; set; }
        public string imageUrl { get; set; }
        public string content { get; set; }
        public int sortOrder { get; set; }
    }

    public class NewsDetailVM
    {
        public News News { get; set; }
        public List<NewsSection> Sections { get; set; }
    }
}


