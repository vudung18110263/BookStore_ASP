using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Book_Shop.Models
{
    public class Item
    {
        public int idProduct { get; set; }
        public string Title { get; set; }
        public string UrlImage { get; set; }
        public string Detail { get; set; }
        public decimal Price { get; set; }
    }
}