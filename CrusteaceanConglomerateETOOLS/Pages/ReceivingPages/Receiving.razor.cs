namespace CrusteaceanConglomerateETOOLS.Pages.ReceivingPages
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
    using ReceivingSystem.BLL;
    using ReceivingSystem.Entities;
    using ReceivingSystem.ViewModels;

    public partial class Receiving
    {
        [Inject]
        private PurchaseOrderService PurchaseOrderService { get; set; }

        private List<PurchaseOrder> outstandingOrders;
        private PurchaseOrder selectedOrder;
        private List<PurchaseOrderDetailViewModel> orderDetailsViewModels;
        private List<UnOrderedItemViewModel> unorderedItemsCart = new List<UnOrderedItemViewModel>();
        private UnOrderedItemViewModel newUnorderedItem = new UnOrderedItemViewModel();
        private int nextCID = 1;
        private string errorMessage = "";
        private string forceCloseReason;


        protected override async Task OnInitializedAsync()
        {
            outstandingOrders = await PurchaseOrderService.GetOutstandingOrdersAsync();
        }
        private void AddToCart()
        {
            newUnorderedItem.CID = nextCID++;
            unorderedItemsCart.Add(newUnorderedItem);
            newUnorderedItem = new UnOrderedItemViewModel(); // Reset for next item entry
        }
        private async Task ForceCloseOrder()
        {
            if (!string.IsNullOrWhiteSpace(forceCloseReason))
            {
                // Logic to force close the order
                if (selectedOrder != null)
                {
                    try
                    {
                        // Assuming you have a method in PurchaseOrderService to handle force close
                        await PurchaseOrderService.ForceCloseOrderAsync(selectedOrder, forceCloseReason);

                        outstandingOrders = await PurchaseOrderService.GetOutstandingOrdersAsync();

                        DeselectOrder();
                        forceCloseReason = "";
                    }
                    catch (Exception ex)
                    {
                        errorMessage = $"Error during force close: {ex.Message}";
                    }
                }
            }
            else
            {
                errorMessage = "Please provide a reason to force close the order.";
            }
        }

        private void RemoveFromCart(UnOrderedItemViewModel item)
        {
            unorderedItemsCart.Remove(item);
        }
        private void SelectOrder(PurchaseOrder order)
        {
            selectedOrder = order;

            orderDetailsViewModels = order.PurchaseOrderDetails
                                          ?.Select(detail => new PurchaseOrderDetailViewModel(detail))
                                          .ToList() ?? new List<PurchaseOrderDetailViewModel>();
        }


        private async Task ProcessOrder()
        {
            if (selectedOrder != null)
            {
                bool isValid = true;
                errorMessage = ""; // Reset the error message

                // Validate each item's received quantity
                foreach (var detail in orderDetailsViewModels)
                {
                    if (detail.ReceivedQuantity <= 0 || detail.ReceivedQuantity > detail.QuantityOutstanding())
                    {
                        isValid = false;
                        errorMessage = "Received Quantity for one or more items is invalid.";
                        break;
                    }
                }

                // Only proceed if the validation passes
                if (isValid)
                {
                    // Call BLL method to process the order
                    await PurchaseOrderService.ProcessReceiveOrderAsync(
                        selectedOrder,
                        orderDetailsViewModels,
                        unorderedItemsCart
                    );

                    // Refresh the list of outstanding orders
                    outstandingOrders = await PurchaseOrderService.GetOutstandingOrdersAsync();

                    // Deselect the order
                    DeselectOrder();
                }
                // No else part needed, error message is already set
            }
        }



        private void DeselectOrder()
        {
            selectedOrder = null;
            orderDetailsViewModels = null;
        }

    }



}
