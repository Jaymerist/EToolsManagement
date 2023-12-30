using CrusteaceanConglomerateETOOLS.Areas;
using Microsoft.AspNetCore.Components;
using PurchaseOrderSystem.BLL;
using PurchaseOrderSystem.ViewModels;
using CrusteaceanConglomerateETOOLS.Pages;
using Microsoft.Identity.Client;
using CrusteaceanConglomerateETOOLS.Shared;
using System.Collections.Generic;
using System.Linq;

namespace CrusteaceanConglomerateETOOLS.Pages.PurchaseOrderPages
{
    public partial class PurchaseOrderHome
    {
        [Inject]
        protected VendorService VendorService { get; set; }
        [Inject]
        protected OrdersService OrdersService { get; set; }
        [Inject]
        protected InventoryService InventoryService { get; set; }

        private PurchaseOrderEditView purchaseOrderEditView;
        private PurchaseOrderView purchaseOrderView;
        private VendorView vendorView;
        private List<ItemView> orderItems;
        private List<ItemView> inventoryItems;
        private List<ItemDetailView> itemDetailViews;
        private int vendorID;
        private List<string> errorList = new List<string>();
        private string successMessage;
        private List<VendorView> allVendors;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            allVendors = VendorService.GetVendors();
        }

        public void FindPurchaseOrder()
        {
            successMessage = string.Empty;

            if(vendorID != 0)
            {
                //Suggest a purchase order if there is none for a vendor
              
                vendorView = allVendors.Where(x => x.VendorID == vendorID).FirstOrDefault();
                purchaseOrderView = OrdersService.SuggestPurchaseOrder(vendorView);

                if (purchaseOrderView == null)
                {
                    //the vendor already has an order.
                    purchaseOrderView = OrdersService.GetVendorPurchaseOrder(vendorID);
                    errorList = new List<string>();
                    successMessage = "An existing order was found!";
                }
                else
                {
                    errorList = new List<string>();
                    successMessage = "No existing order. A suggested order has been created.";
                }
                //reset lists
                orderItems = new List<ItemView>();
                inventoryItems = new List<ItemView>();
                orderItems = purchaseOrderView.Items;
                inventoryItems = InventoryService.FetchInventoryBy(purchaseOrderView, vendorID);
            }
            else
            {
                successMessage = null;
                errorList.Add("Please make a vendor selection.");
            } 
        }

        private async Task PriceCalc()
        {
            List<decimal> subtotal = new List<decimal>();
            foreach(var item in orderItems)
            {
                subtotal.Add(item.Price * item.QTO);
            }
            purchaseOrderView.SubTotal = subtotal.Sum();
            purchaseOrderView.GST = purchaseOrderView.SubTotal * 0.05m;
            await InvokeAsync(StateHasChanged);
        }


        private async Task RemoveItem(ItemView item)
        {
            //search for our customers
            orderItems.Remove(item);
            inventoryItems.Add(item);

            await InvokeAsync(StateHasChanged);
        }

        private async Task AddItem(ItemView item)
        {
            //search for our customers
            orderItems.Add(item);
            if(item.QTO <= 0)
            {
                item.QTO = 1;
            }
            
            inventoryItems.Remove(item);

            await InvokeAsync(StateHasChanged);
        }

        private void Update()
        {
            try
            {
                //reset item details view and error list 
                errorList.Clear();
                itemDetailViews = new List<ItemDetailView>();
                //save item details for each item in the order list
                foreach (var item in orderItems)
                {
                    var itemDetail = new ItemDetailView();
                    itemDetail.StockItemID = item.StockItemID;
                    itemDetail.QTO = item.QTO;
                    itemDetail.Price = item.Price;

                    itemDetailViews.Add(itemDetail);
                }

                //create PurchaseOrderEditView
                purchaseOrderEditView = new PurchaseOrderEditView();
                purchaseOrderEditView.PurchaseOrderID = purchaseOrderView.PurchaseOrderID;
                purchaseOrderEditView.VendorID = vendorID;
                purchaseOrderEditView.EmployeeID = 1;
                purchaseOrderEditView.ItemDetails = itemDetailViews;
                purchaseOrderEditView.SubTotal = itemDetailViews.Sum(x => x.Price * x.QTO);
                purchaseOrderEditView.GST = purchaseOrderEditView.SubTotal * (decimal)0.05;

                //validation
                if(itemDetailViews.Any(x=>x.QTO <= 0))
                {
                    errorList.Add("Quantity must be positive and over 0.");
                }
                if(itemDetailViews.Any(x=>x.Price <= 0))
                {
                    errorList.Add("Price must be positive and over 0.");
                }

                if(errorList.Count == 0) 
                {
                    purchaseOrderView.PurchaseOrderID = OrdersService.UpdatePurchaseOrder(purchaseOrderEditView).PurchaseOrderID;
                    
                    successMessage = "Saved successfully!";
                }
                
            }
            #region catch all exceptions 
            catch (AggregateException ex)
            {
                successMessage = null;
                foreach (var error in ex.InnerExceptions)
                {
                    
                    errorList.Add(error.Message + Environment.NewLine);
                }
            }
            catch (ArgumentNullException ex)
            {
                successMessage = null;
                errorList.Add(GetInnerException(ex).Message);
            }
            catch (Exception ex)
            {
                successMessage = null;
                errorList.Add(GetInnerException(ex).Message);
            }
            #endregion
            
        }

        private void Place()
        {
            try
            {   //reset item details view
                itemDetailViews = new List<ItemDetailView>();
                successMessage = null;
                //save item details for each item in the order list
                foreach (var item in orderItems)
                {
                    var itemDetail = new ItemDetailView();
                    itemDetail.StockItemID = item.StockItemID;
                    itemDetail.QTO = item.QTO;
                    itemDetail.Price = item.Price;

                    itemDetailViews.Add(itemDetail);
                }

                //create PurchaseOrderEditView
                purchaseOrderEditView = new PurchaseOrderEditView();
                purchaseOrderEditView.PurchaseOrderID = purchaseOrderView.PurchaseOrderID;
                purchaseOrderEditView.VendorID = vendorID;
                purchaseOrderEditView.EmployeeID = 1;
                purchaseOrderEditView.ItemDetails = itemDetailViews;
                purchaseOrderEditView.SubTotal = itemDetailViews.Sum(x => x.Price * x.QTO);
                purchaseOrderEditView.GST = purchaseOrderEditView.SubTotal * (decimal)0.05;

                //validation
                //there needs to be items in order to place an order
                if (itemDetailViews.Count == 0)
                {
                    errorList.Add("You cannot place an order without any items!");
                }
                else
                {
                    if (itemDetailViews.Any(x => x.QTO <= 0))
                    {
                        errorList.Add("Quantity must be positive and over 0.");
                    }
                    if (itemDetailViews.Any(x => x.Price <= 0))
                    {
                        errorList.Add("Price must be positive and over 0.");
                    }
                }
                
                if(errorList.Count() == 0)
                {
                    //update the order before placing
                    purchaseOrderEditView.PurchaseOrderID = OrdersService.UpdatePurchaseOrder(purchaseOrderEditView).PurchaseOrderID;

                    OrdersService.PlacePurchaseOrder(purchaseOrderEditView);
                    Clear();
                    successMessage = "Your order has been placed.";
                }
            }
            #region catch all exceptions 
            catch (AggregateException ex)
            {
                foreach (var error in ex.InnerExceptions)
                {
                    successMessage = null;
                    errorList.Add(error.Message + "  ");
                }
            }
            catch (ArgumentNullException ex)
            {
                successMessage = null;
                errorList.Add(GetInnerException(ex).Message);
            }
            catch (Exception ex)
            {
                successMessage = null;
                errorList.Add(GetInnerException(ex).Message);
            }
            #endregion

        }


        private async Task Clear()
        {
            purchaseOrderView = null;
            vendorID = 0;
            errorList = new List<string>();
            successMessage = null;
            orderItems = new List<ItemView>();
            inventoryItems = new List<ItemView>();
            itemDetailViews = new List<ItemDetailView>();
            await InvokeAsync(StateHasChanged);
        }

        private async Task Delete()
        {
            OrdersService.DeletePurchaseOrder(purchaseOrderView.PurchaseOrderID);
            await Clear();
            successMessage = "Purchase order deleted.";
            await InvokeAsync(StateHasChanged);
        }

        private Exception GetInnerException(Exception ex)
        {
            while (ex.InnerException != null)
                ex = ex.InnerException;
            return ex;
        }

    }
}
