#nullable disable
using SaleSystem.ViewModels;

namespace CrusteaceanConglomerateETOOLS.Data
{
    public class AppState
    {
        public CategoryView CategoryView { get; set; }
        public StockItemView StockItemView { get; set; }
        public SaleView SaleView { get; set; }
        public ShoppingCartView ShoppingCartView { get; set; }
    }
}
