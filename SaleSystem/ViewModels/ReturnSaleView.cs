using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaleSystem.ViewModels
{
    public class ReturnSaleView
    {
        public int SaleID { get; set; }
        public int EmployeeID { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal SubTotal { get; set; }
        public int CouponID { get; set; }
        public string Reason { get; set; }
        public List<ReturnSaleDetailCartView> ReturnSaleDetails { get; set; } = new();
    }
}
