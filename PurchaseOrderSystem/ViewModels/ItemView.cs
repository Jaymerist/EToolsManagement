namespace PurchaseOrderSystem.ViewModels
{
    public class ItemView
    {
        public int PurchaseOrderDetailID { get; set; }
        public int StockItemID { get; set; }
        public string Description { get; set; }
        public int QOH { get; set; } // Quanity on Hand
        public int ROL { get; set; } // Reoreder Level
        public int QOO { get; set; } // Quantity on Order
        public int QTO { get; set; } // Quantity to Order
        public decimal Price { get; set; }
    }

}
