//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Book_Shop.Models
{
    using System;
    using System.Collections.Generic;

    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class Product
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        [Key]
        public int id { get; set; }
        public string name { get; set; }
        public string image { get; set; }
        public string description { get; set; }
        public string category { get; set; }
        public Nullable<int> price { get; set; }
        public Nullable<int> rate { get; set; }
        public Nullable<int> stock { get; set; }
        public Nullable<int> authorId { get; set; }

        public Product(Product product)
        {
            this.id = product.id;
            this.name = product.name;
            this.image = product.image;
            this.description = product.description;
            this.category = product.category;
            this.price = product.price;
            this.rate = product.rate;
            this.stock = product.stock;
            this.authorId = product.authorId;
        }
        public Product()
        {
            this.Order_Product = new HashSet<Order_Product>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [ForeignKey("Order_Product")]
        public virtual ICollection<Order_Product> Order_Product { get; set; }
        public virtual User User { get; set; }
    }
}

