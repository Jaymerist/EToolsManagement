using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaleSystem.ViewModels
{
    public class ShoppingCartView
    {
        public int StockItemID { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal SellingPrice { get; set; }
        public int QuantityOnHand { get; set; }
        public decimal Total {  get; set; }

    }
}
