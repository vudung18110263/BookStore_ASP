using System;
using System.Collections.Generic;
using System.Linq;

namespace Book_Shop.Models
{
    public class Order_Detail : IComparable<Order_Detail>
    {
        private Book_StoreEntities2 db = new Book_StoreEntities2();
        public int id { get; set; }
        public int userid { get; set; }
        public int? promoid { get; set; }
        public string status { get; set; }
        public System.DateTime date { get; set; }
        public string shippingAddess { get; set; }
        public string payment { get; set; }
        public int? promoValue { get; set; }
        public List<OrderProJoinProduct> orderProduct { get; set; }
        public int PriceALl { get; set; }
        public string shippingType { get; set; }

        public string UserMail { get; set; }
        public string UserFullName { get; set; }
        public int CompareTo(Order_Detail obj) // OverRight phương thức CompareTo của Interface IComparable
        {
            return obj.date.CompareTo(this.date); //Phương thức CompareTo này có sẵn với các kiểu cơ bản như Integer, String.
        }
        public Order_Detail() { }
        public Order_Detail(Order orther, List<OrderProJoinProduct> ListOrtherProduct, int priceALl)
        {
            id = orther.id;
            userid = orther.userid;
            promoid = orther.promoid;
            status = orther.status;
            date = orther.date;
            shippingAddess = orther.shippingAddess;
            payment = orther.payment;
            orderProduct = ListOrtherProduct;
            var user = db.Users.Where(x => x.id == orther.userid).FirstOrDefault();
            UserFullName = user.fullname;
            UserMail = user.mail;
            var promo = db.PromoCodes.Where(x => x.id == promoid).FirstOrDefault();
            if (promo != null)
            {
                promoValue = promo.value;
            }
            else
                promoValue = 0;
            PriceALl = priceALl-Convert.ToInt32(promoValue);
            shippingType = orther.shippingType;
        }

    }
}