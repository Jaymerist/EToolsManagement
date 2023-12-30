using System.ComponentModel.DataAnnotations;

namespace RentalSystem.ViewModels
{
    public class RentalsView
    {
        public int RentalID { get; set; }
        public int CustomerID { get; set; }
        public int EmployeeID { get; set; }
        public int? CouponID { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public DateTime RentalDateOut { get; set; }
        public DateTime RentalDateIn { get; set; }
        public string PaymentType { get; set; }
        public List<RentalDetailView> RentalDetails { get; set; }
        public CouponView Coupon { get; set; }
    }
}
