using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaleSystem.ViewModels
{
    public class ReturnSaleDetailCartView
    {
        public int StockItemID { get; set; }
        public string Description { get; set; }
        public int OriginalQty { get; set; }
        public decimal SellingPrice { get; set; }
        public int PreviousReturnQty { get; set; }
        public int QtyReturnNow { get; set; }

    }
}
