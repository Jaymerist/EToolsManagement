using SaleSystem.BLL;
using SaleSystem.ViewModels;
using Microsoft.AspNetCore.Components;
using CrusteaceanConglomerateETOOLS.Data;
using SaleSystem.Entities;
using Microsoft.AspNetCore.Components.Web;

namespace CrusteaceanConglomerateETOOLS.Pages.SalePages
{
    public partial class SaleHome
    {
        #region Fields

        // Variables
        private int categoryID;
        private int stockItemID = 0;
        private bool checkOutBtn = false;
        private int quantity { get; set; }
        private CategoryView category = new();
        private StockItemView stock = new();
        private SaleView newSale = new();
        private ShoppingCartView cart = new();
        [Parameter]
        public EventCallback<string> TextChanged { get; set; }

        // Error handling
        private string feedBackMessage;
        private string errorMessage;
        private bool hasFeedback => !string.IsNullOrWhiteSpace(feedBackMessage);
        private bool hasError => !string.IsNullOrWhiteSpace(errorMessage);
        List<Exception> errorList = new List<Exception>();

        // Injects
        [Inject]
        protected SaleService SaleService { get; set; }
        [Inject]
        protected RefundService RefundService { get; set; }
        [Inject]
        protected NavigationManager NavigationManager { get; set; }
        [Inject]
        AppState AppState { get; set; }


        // Lists
        private List<CategoryView> categoryList;
        private List<StockItemView> categoryItems = new();
        private List<ShoppingCartView> cartItems = new();

        #endregion

        #region Methods

        protected override async Task OnInitializedAsync()
        {
            
            await base.OnInitializedAsync();
            try
            {
                errorList.Clear();
                errorMessage = string.Empty;
                feedBackMessage = String.Empty;
                categoryList = SaleService.GetCategories();
                await InvokeAsync(StateHasChanged);
            }
            catch (AggregateException ex)
            {
                errorList.Add(GetInnerException(ex));
            }
        }

        private async void SearchCategoryItems()
        {

            await base.OnInitializedAsync();
            try
            {
                categoryItems = SaleService.GetItemsByCategoryID(categoryID);
                await InvokeAsync(StateHasChanged);
            }
            catch(ArgumentNullException ex)
            {
                errorList.Add(GetInnerException(ex));
            }
        }

        private async Task AddtoCart(int stockID, StockItemView items)
        {
           errorList.Clear();
           errorMessage = string.Empty;
           feedBackMessage = String.Empty;
           ShoppingCartView cartList = SaleService.GetCart(stockID);
           StockItemView newStockLine = new StockItemView();
           newStockLine.QuantityOnHand = cartList.QuantityOnHand;

           if (cartItems.Any(x => x.StockItemID == stockID))
           {
               errorMessage = $"\"{cartList.Description}\" already exists in the shopping cart!";
           }
           else if (items.Quantity > newStockLine.QuantityOnHand)
           {
                errorMessage = $"Cannot select more than {newStockLine.QuantityOnHand} for this item";
           }
           else if (items.Quantity == 0)
           {
                errorMessage = $"You have selected {items.Quantity} items";
           }
           else
           {
                newStockLine.StockItemID = cartList.StockItemID;
                newStockLine.Description = cartList.Description;
                cartList.Quantity = items.Quantity;
                newStockLine.SellingPrice = cartList.SellingPrice;
                cartItems.Add(cartList);
                cartList.Total = cartList.SellingPrice * items.Quantity;

                feedBackMessage = $"Successfully added {cartList.Description} x{cartList.Quantity} ";
                await InvokeAsync(StateHasChanged);
                //items.Quantity = 0;
                CheckOut(cartList.StockItemID);
           }

        }

        private async Task RemoveFromCart(int stockID)
        {
            errorList.Clear();
            errorMessage = string.Empty;
            feedBackMessage = String.Empty;
            ShoppingCartView removeStock = cartItems.Where(x => x.StockItemID == stockID).FirstOrDefault();
            cartItems.Remove(removeStock);
            feedBackMessage = $"Successfully removed {removeStock.Description} x{removeStock.Quantity}";
        }

        private void Refresh()
        {
            
            StateHasChanged();
        }

        private void UpdateCartItem(ShoppingCartView cartItem)
        {
            StateHasChanged();
            cartItem.Total = cartItem.Quantity * Math.Round(cartItem.SellingPrice, 2);
            
        }

        private void HandleMouseDown(MouseEventArgs e)
        {   
            StateHasChanged();
        }

        private void CheckOut(int stockID)
        {
            checkOutBtn = true;
            ShoppingCartView cartList = SaleService.GetCart(stockID);
            SaleView newSale = new SaleView();
            //newSale = SaleService.SaveSales(saleID);
            //cartList.StockItemID = cart.StockItemID;
            cart.StockItemID = cartList.StockItemID;
            cart.SellingPrice = cartList.SellingPrice;
            newSale.SubTotal = cartList.Total;
            newSale.TaxAmount = 0.05m;
            

            StateHasChanged();
        }

        private void PathToCart()
        {
            AppState.StockItemView = stock;
            AppState.ShoppingCartView = cart;
            NavigationManager.NavigateTo($"/SalePages/CartHome/");
        }

        private void PathToItems()
        {
            NavigationManager.NavigateTo($"/SalePages/SaleHome");
        }

        public static Exception GetInnerException(System.Exception ex)
        {
            while (ex.InnerException != null)
                ex = ex.InnerException;
            return ex;
        }

        #endregion



    }
}
