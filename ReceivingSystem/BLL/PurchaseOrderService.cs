using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReceivingSystem.DAL;
using ReceivingSystem.Entities;
using ReceivingSystem.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ReceivingSystem.BLL
{
    public class PurchaseOrderService
    {
        private readonly eTools2023Context _context;

        public PurchaseOrderService(eTools2023Context context)
        {
            _context = context;
        }

        public async Task<List<PurchaseOrder>> GetOutstandingOrdersAsync()
        {
            return await _context.PurchaseOrders
                .Include(po => po.Vendor)
                .Include(po => po.PurchaseOrderDetails)
                    .ThenInclude(pod => pod.StockItem)
                .Include(po => po.PurchaseOrderDetails)
                    .ThenInclude(pod => pod.ReceiveOrderDetails) 
                .Where(po => !po.Closed && po.OrderDate.HasValue)
                .ToListAsync();
        }
        public async Task ForceCloseOrderAsync(PurchaseOrder purchaseOrder, string reason)
        {
            if (purchaseOrder != null && !string.IsNullOrWhiteSpace(reason))
            {
                purchaseOrder.Closed = true;
                purchaseOrder.Notes = reason; 
                _context.PurchaseOrders.Update(purchaseOrder);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ProcessReceiveOrderAsync(PurchaseOrder purchaseOrder,
                                                   List<PurchaseOrderDetailViewModel> receivedOrderDetails,
                                                   List<UnOrderedItemViewModel> unorderedItems)
        {
            // Create a new ReceiveOrder entry
            var receiveOrder = new ReceiveOrder
            {
                PurchaseOrderID = purchaseOrder.PurchaseOrderID,
                ReceiveDate = DateTime.Now,
                EmployeeID = 1, // Replace with the actual employee ID
                RemoveFromViewFlag = false,
            };

            _context.ReceiveOrders.Add(receiveOrder);
            await _context.SaveChangesAsync();

            // Process each received item
            foreach (var receivedItem in receivedOrderDetails)
            {
                var receiveOrderDetail = new ReceiveOrderDetail
                {
                    ReceiveOrderID = receiveOrder.ReceiveOrderID,
                    PurchaseOrderDetailID = receivedItem.PurchaseOrderDetailID,
                    QuantityReceived = receivedItem.ReceivedQuantity,
                    RemoveFromViewFlag = false
                };
                _context.ReceiveOrderDetails.Add(receiveOrderDetail);

                var stockItem = await _context.StockItems.FindAsync(receivedItem.StockItemID);
                stockItem.QuantityOnHand += receivedItem.ReceivedQuantity;
                stockItem.QuantityOnOrder -= receivedItem.ReceivedQuantity;
                _context.StockItems.Update(stockItem);

                if (receivedItem.ReturnedQuantity > 0)
                {
                    var returnedOrderDetail = new ReturnedOrderDetail
                    {
                        ReceiveOrderID = receiveOrder.ReceiveOrderID,
                        PurchaseOrderDetailID = receivedItem.PurchaseOrderDetailID,
                        Quantity = receivedItem.ReturnedQuantity,
                        Reason = receivedItem.ReturnReason, // Reason directly from input
                        ItemDescription = receivedItem.StockItemDescription,
                        VendorStockNumber = receivedItem.StockItemVendorPartNumber,
                        RemoveFromViewFlag = false,
                    };
                    _context.ReturnedOrderDetails.Add(returnedOrderDetail);
                }
            }

            // Process unordered items
            foreach (var unorderedItem in unorderedItems)
            {
                var returnedUnorderedItem = new ReturnedOrderDetail
                {
                    ReceiveOrderID = receiveOrder.ReceiveOrderID,
                    ItemDescription = unorderedItem.Description,
                    Quantity = unorderedItem.Quantity,
                    VendorStockNumber = unorderedItem.VendorPartNumber,
                    RemoveFromViewFlag = false,
                };
                _context.ReturnedOrderDetails.Add(returnedUnorderedItem);
            }

            // Check if the order is complete and close if necessary
            if (!purchaseOrder.PurchaseOrderDetails.Any(d => d.Quantity > d.ReceiveOrderDetails.Sum(r => r.QuantityReceived)))
            {
                purchaseOrder.Closed = true;
                _context.PurchaseOrders.Update(purchaseOrder);
            }

            await _context.SaveChangesAsync();
        }
    }
}
