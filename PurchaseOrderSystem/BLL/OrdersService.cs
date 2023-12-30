#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PurchaseOrderSystem.DAL;
using PurchaseOrderSystem;
using PurchaseOrderSystem.ViewModels;
using PurchaseOrderSystem.BLL;
using PurchaseOrderSystem.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace PurchaseOrderSystem.BLL
{
    public class OrdersService
    {
        private readonly eTools2023Context? _eTools2023Context;
        internal OrdersService(eTools2023Context eToolsContext)
        {
            _eTools2023Context = eToolsContext;
        }
        public PurchaseOrderView GetVendorPurchaseOrder(int vendorID)
        {
            return _eTools2023Context.PurchaseOrders
                .Where(x => x.VendorID == vendorID && x.OrderDate == null && x.RemoveFromViewFlag == false)
                .Select(x => new PurchaseOrderView
                {
                    Vendor = new VendorView
                    {
                        VendorID = x.Vendor.VendorID,
                        VendorName = x.Vendor.VendorName,
                        Phone = x.Vendor.Phone,
                        Address = x.Vendor.Address,
                        City = x.Vendor.City,
                        Province = x.Vendor.ProvinceID,
                        PostalCode = x.Vendor.PostalCode
                    },
                    PurchaseOrderID = x.PurchaseOrderID,
                    Items = x.PurchaseOrderDetails.Where(x=>x.RemoveFromViewFlag == false).Select(pd => new ItemView
                    {
                        PurchaseOrderDetailID = pd.PurchaseOrderDetailID,
                        StockItemID = pd.StockItem.StockItemID,
                        Description = pd.StockItem.Description,
                        QOH = pd.StockItem.QuantityOnHand,
                        ROL = pd.StockItem.ReOrderLevel,
                        QOO = pd.StockItem.QuantityOnOrder,
                        QTO = pd.Quantity,
                        Price = pd.PurchasePrice,
                        
                    }).ToList(),
                    SubTotal = x.SubTotal,
                    GST = x.TaxAmount
                })
                .FirstOrDefault();
        }

        //Populate new order 
        public PurchaseOrderView SuggestPurchaseOrder(VendorView vendor)
        {

            //we need a container to hold x number of Exception messages
            List<Exception> errorList = new List<Exception>();

            //check if Vendor exists
            if (!_eTools2023Context.Vendors.Any(x => x.VendorID == vendor.VendorID))
            {
                errorList.Add(new Exception("Vendor does not exist"));
            }

            if (_eTools2023Context.PurchaseOrders.Where(x => x.VendorID == vendor.VendorID && x.OrderDate == null && x.RemoveFromViewFlag == false).Count() == 0)
            {
                //There are no active orders
                //Create a suggested PO
                PurchaseOrderView newOrder = new PurchaseOrderView();

                newOrder.Vendor = vendor;

                //Suggest items
                //get inventory of vendor
                InventoryService inventory = new InventoryService(_eTools2023Context);
                
                var Inventory = inventory.FetchInventoryBy(vendor.VendorID);
                newOrder.Items = new List<ItemView>();
                foreach (ItemView item in Inventory)
                {
                    //add if ReorderLevel is higher than QuantityOnHand
                    if (item.QTO > 0)
                    {
                        newOrder.Items.Add(item);
                    }

                }

                newOrder.SubTotal = newOrder.Items.Sum(x => x.Price * x.QTO);
                newOrder.GST = newOrder.SubTotal * 0.05m;

                if (errorList.Count > 0)
                {
                    // Clear changes to maintain data integrity.
                    _eTools2023Context.ChangeTracker.Clear();
                    string errorMsg = "Please check error message(s)";
                    throw new AggregateException(errorMsg, errorList);
                }
                else
                {
                    return newOrder;
                }

            }
            else
            {

                if (errorList.Count > 0)
                {
                    // Clear changes to maintain data integrity.
                    _eTools2023Context.ChangeTracker.Clear();
                    string errorMsg = "Please check error message(s)";
                    throw new AggregateException(errorMsg, errorList);
                }
                else
                {
                    return null;
                }
                //there is no need to suggest an order. Open the existing order.

            }
        }

        #region CRUD 

        //Delete item from list
        public void RemovePurchaseOrderLine(ItemView item, int poID)
        {
            //we need a container to hold x number of Exception messages
            List<Exception> errorList = new List<Exception>();

            //Rules:
            //The user selects a stock item from the table and does not input anything about the item. It should be assumed that the item
            //exists in the database and has valid data if it's being shown to the user in the first place.

            //Check if item does not already exist in purchase order items list
            var itemLine = _eTools2023Context.PurchaseOrderDetails.Where(x => x.PurchaseOrderID == poID && x.StockItemID == item.StockItemID).FirstOrDefault();

            if (!_eTools2023Context.PurchaseOrderDetails.Where(x => x.PurchaseOrderID == poID).Any(existingItem => existingItem.StockItemID == item.StockItemID))
            {
                // Item does not exist in the purchase order, throw an exception or handle accordingly
                errorList.Add(new Exception("Unable to remove item from purchase order."));
            }

            //set remove from view flag to true to show the user that the purchase item is gone (not deleted permanently from the DB)
            itemLine.RemoveFromViewFlag = true;

            if (errorList.Count > 0)
            {
                // Clear changes to maintain data integrity.
                _eTools2023Context.ChangeTracker.Clear();
                string errorMsg = "Please check error message(s)";
                throw new AggregateException(errorMsg, errorList);
            }
            else
            {
                // Persist changes to the database.
                _eTools2023Context.SaveChanges();
            }
        }

        //Place order

        public void PlacePurchaseOrder(PurchaseOrderEditView po)
        {
            //we need a container to hold x number of Exception messages
            List<Exception> errorList = new List<Exception>();

            var order = _eTools2023Context.PurchaseOrders.Where(x => x.PurchaseOrderID == po.PurchaseOrderID).FirstOrDefault();
            //make sure the purchase order has not already been placed
            //purchase orders cannot be placed if there are no items
            if (order.OrderDate == null)
            {
                //purchase orders cannot be placed if there are no items
                if (_eTools2023Context.PurchaseOrderDetails.Where(x => x.PurchaseOrderID == order.PurchaseOrderID).Count() != 0)
                {
                    //purchase orders cannot be placed with items that do not belong to that vendor
                    if (_eTools2023Context.PurchaseOrderDetails.Where(x => x.PurchaseOrderID == order.PurchaseOrderID).Any(x => x.StockItem.VendorID != po.VendorID) == true)
                    {
                        throw new ArgumentException("This order cannot be placed as it contains items from one or more other vendors.");
                    }
                    //will set the OrderDate for the current order
                    order.OrderDate = DateTime.Now;
                    _eTools2023Context.PurchaseOrders.Update(order);

                    foreach (ItemDetailView itemLine in po.ItemDetails)
                    {
                        PurchaseOrderDetail poDetail = new PurchaseOrderDetail();
                        poDetail.StockItemID = itemLine.StockItemID;
                        poDetail.PurchasePrice = itemLine.Price;
                        poDetail.Quantity = itemLine.QTO;

                        //update the QuantityOnOrder for each stock item on the purchase order if the order is being placed
                        StockItem item = _eTools2023Context.StockItems.Where(x => x.StockItemID == poDetail.StockItemID).FirstOrDefault();
                        item.QuantityOnOrder += poDetail.Quantity;

                        _eTools2023Context.StockItems.Update(item);

                    }

                    if (errorList.Count > 0)
                    {
                        // Clear changes to maintain data integrity.
                        _eTools2023Context.ChangeTracker.Clear();
                        string errorMsg = "Please check error message(s)";
                        throw new AggregateException(errorMsg, errorList);
                    }
                    else
                    {
                        // Persist changes to the database.
                        _eTools2023Context.SaveChanges();
                    }
                }

                else
                {
                    errorList.Add(new Exception("The order cannot be placed without any items on it."));
                }
            }
            else
            {
                errorList.Add(new Exception("The order has already been placed. It cannot be placed again."));
            }

        }

        //This can be used for the Update or before Place 
        public PurchaseOrderView UpdatePurchaseOrder(PurchaseOrderEditView order)
        {
            //we need a container to hold x number of Exception messages
            List<Exception> errorList = new List<Exception>();
            PurchaseOrder newOrder = new PurchaseOrder();
            //Rules:
            //The order ID must exist

            //Suggested purchase orders should be saved, not updated.
            if (order.PurchaseOrderID == 0)
            {
                newOrder.VendorID = order.VendorID;
                newOrder.EmployeeID = order.EmployeeID;
                newOrder.TaxAmount = Math.Round(order.GST, 2);
                newOrder.SubTotal = order.SubTotal;
                newOrder.Employee = _eTools2023Context.Employees.Where(x => x.EmployeeID == newOrder.EmployeeID).FirstOrDefault();
                newOrder.Vendor = _eTools2023Context.Vendors.Where(x => x.VendorID == newOrder.VendorID).FirstOrDefault();

                //Save the item details
                foreach (ItemDetailView itemLine in order.ItemDetails)
                {
                    PurchaseOrderDetail poDetail = new PurchaseOrderDetail();
                    poDetail.StockItemID = itemLine.StockItemID;
                    poDetail.PurchasePrice = itemLine.Price;
                    poDetail.Quantity = itemLine.QTO;
                    newOrder.PurchaseOrderDetails.Add(poDetail);

                }
                _eTools2023Context.PurchaseOrders.Add(newOrder);
            }
            else
            {
                //Purchase order ID needs to exist and not be deleted
                if (!_eTools2023Context.PurchaseOrders.Any(x => x.PurchaseOrderID == order.PurchaseOrderID && x.RemoveFromViewFlag == false))
                {
                    errorList.Add(new Exception("Purchase order does not exist in database."));
                }
                newOrder = _eTools2023Context.PurchaseOrders.Where(x => x.PurchaseOrderID == order.PurchaseOrderID).FirstOrDefault();
                newOrder.EmployeeID = order.EmployeeID;
                newOrder.TaxAmount = Math.Round(order.GST, 2);
                newOrder.SubTotal = order.SubTotal;

                //remove purchase order items that exist in the db but are not on the new list of items
                List<PurchaseOrderDetail> existingOrderList = _eTools2023Context.PurchaseOrderDetails.Where(x => x.PurchaseOrderID == order.PurchaseOrderID && x.RemoveFromViewFlag == false).ToList();

                foreach (var item in existingOrderList)
                {
                    if (!order.ItemDetails.Any(x => x.StockItemID == item.StockItemID))
                    {
                        var deleteItem = new ItemView();
                        deleteItem.StockItemID = item.StockItemID;
                        RemovePurchaseOrderLine(deleteItem, order.PurchaseOrderID);
                    }
                }

                //Save the item details
                foreach (ItemDetailView itemLine in order.ItemDetails)
                {
                    PurchaseOrderDetail poDetail = new PurchaseOrderDetail();
                    if (_eTools2023Context.PurchaseOrderDetails.Where(x => x.PurchaseOrderID == order.PurchaseOrderID && x.StockItemID == itemLine.StockItemID).Count() != 0)
                    {
                        poDetail = _eTools2023Context.PurchaseOrderDetails.Where(x => x.PurchaseOrderID == order.PurchaseOrderID && x.StockItemID == itemLine.StockItemID).FirstOrDefault();
                        
                    }
                    
                    poDetail.PurchaseOrderID = order.PurchaseOrderID;
                    poDetail.StockItemID = itemLine.StockItemID;
                    poDetail.PurchasePrice = itemLine.Price;
                    poDetail.Quantity = itemLine.QTO;

                    //check if line already exists to add or update it
                    if (poDetail.PurchaseOrderDetailID != 0)
                    {
                        _eTools2023Context.PurchaseOrderDetails.Update(poDetail);
                    }
                    else
                    {
                        _eTools2023Context.PurchaseOrderDetails.Add(poDetail);
                    }

                }

                _eTools2023Context.PurchaseOrders.Update(newOrder);
            }

            if (errorList.Count > 0)
            {
                // Clear changes to maintain data integrity.
                _eTools2023Context.ChangeTracker.Clear();
                string errorMsg = "Please check error message(s)";
                throw new AggregateException(errorMsg, errorList);
            }
            else
            {
                // Persist changes to the database.
                _eTools2023Context.SaveChanges();
            }


            //save to PurchaseOrderView to redisplay 
            var newPOView = GetVendorPurchaseOrder(newOrder.VendorID);
            return newPOView;

        }

        //Soft delete a purchase order
        public bool DeletePurchaseOrder(int purchaseOrderID)
        {
            //we need a container to hold x number of Exception messages
            List<Exception> errorList = new List<Exception>();

            //Rules:
            //The order ID must exist and not already be deleted (RemoveFromViewFlag on)
            if (!_eTools2023Context.PurchaseOrders.Any(x => x.PurchaseOrderID == purchaseOrderID && x.RemoveFromViewFlag == false))
            {
                errorList.Add(new Exception("Purchase order does not exist in database."));
            }
            //Placed orders cannot be deleted
            PurchaseOrder order = _eTools2023Context.PurchaseOrders.Where(x => x.PurchaseOrderID == purchaseOrderID).FirstOrDefault();
            if (order.OrderDate != null)
            {
                // Order has already been placed, throw an error
                errorList.Add(new Exception("Placed orders cannot be deleted."));
            }

            //set remove from view flag to true 
            order.RemoveFromViewFlag = true;

            _eTools2023Context.PurchaseOrders.Update(order);

            if (errorList.Count > 0)
            {
                // Clear changes to maintain data integrity.
                _eTools2023Context.ChangeTracker.Clear();
                string errorMsg = "Please check error message(s)";
                throw new AggregateException(errorMsg, errorList);
            }
            else
            {
                // Persist changes to the database.
                _eTools2023Context.SaveChanges();
                return true; //use this to check if the record was deleted and send a confirmation message to the user.
            }
        }
        #endregion
    }
}
