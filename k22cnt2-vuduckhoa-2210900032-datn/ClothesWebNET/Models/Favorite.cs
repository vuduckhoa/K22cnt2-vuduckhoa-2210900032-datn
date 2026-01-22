using System;

namespace ClothesWebNET.Models
{
    // NOTE: Project is Database-First (EDMX). This model is used for SQL projection only.
    // Table schema is defined in Scripts/CreateFavoriteTable.sql
    public class Favorite
    {
        public string idUser { get; set; }
        public string idProduct { get; set; }
        public DateTime? createdAt { get; set; }
    }

    public class FavoriteProductDTO
    {
        public string idProduct { get; set; }
        public string nameProduct { get; set; }
        public double price { get; set; }
        public string URLImage { get; set; }
    }
}


