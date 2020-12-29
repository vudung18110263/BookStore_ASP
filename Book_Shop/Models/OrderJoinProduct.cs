namespace Book_Shop.Models
{
    public class OrderProJoinProduct
    {
        public int id { get; set; }
        public int orderId { get; set; }
        public int productId { get; set; }
        public int quantity { get; set; }
        public int price { get; set; }

        public string nameProduct { get; set; }
        public string imageProduct { get; set; }
        public string descriptionProduct { get; set; }
        public string categoryProduct { get; set; }
        public string author { get; set; }
        public int? authorid { get; set; }

        public OrderProJoinProduct() { }
        public OrderProJoinProduct(Order_Product order_Product, Product product)
        {
            id = order_Product.id;
            orderId = order_Product.orderId;
            productId = order_Product.productId;
            quantity = order_Product.quantity;
            price = order_Product.price;

            nameProduct = product.name;
            imageProduct = product.image;
            descriptionProduct = product.description;
            categoryProduct = product.category;
            author = product.User.fullname;
            authorid = product.authorId;

        }
    }
}