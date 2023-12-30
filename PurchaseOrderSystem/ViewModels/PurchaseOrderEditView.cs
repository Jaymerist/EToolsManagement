namespace PurchaseOrderSystem.ViewModels
{
    public class PurchaseOrderEditView
    {
        public int PurchaseOrderID { get; set; }
        public int VendorID { get; set; }
        public int EmployeeID { get; set; }
        public List<ItemDetailView> ItemDetails { get; set; }
        public decimal SubTotal { get; set; }
        public decimal GST { get; set; }
    }
}
