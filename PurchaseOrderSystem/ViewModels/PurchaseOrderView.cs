namespace PurchaseOrderSystem.ViewModels
{
    public class PurchaseOrderView
    {
        public VendorView Vendor { get; set; }
        public int PurchaseOrderID { get; set; }
        public List<ItemView> Items { get; set; }
        public decimal SubTotal { get; set; }
        public decimal GST { get; set; }
    }

}
