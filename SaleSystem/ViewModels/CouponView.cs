using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaleSystem.ViewModels
{
    public class CouponView
    {
        public int CouponID { get; set; }
        public string CouponValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Discount {  get; set; }
        public string Message { get; set; }

    }
}
