using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReceivingSystem.Entities;

namespace ReceivingSystem.ViewModels
{
    

    public class PurchaseOrderDetailViewModel : PurchaseOrderDetail
    {
        public int ReceivedQuantity { get; set; }
        public int ReturnedQuantity { get; set; }
        public string ReturnReason { get; set; }
        public string StockItemDescription { get; set; }
        public string StockItemVendorPartNumber { get; set; }

        public PurchaseOrderDetailViewModel(PurchaseOrderDetail detail)
        {
            PurchaseOrderDetailID = detail.PurchaseOrderDetailID;
            PurchaseOrderID = detail.PurchaseOrderID;
            StockItemID = detail.StockItemID;
            PurchasePrice = detail.PurchasePrice;
            Quantity = detail.Quantity;
            RemoveFromViewFlag = detail.RemoveFromViewFlag;
            PurchaseOrder = detail.PurchaseOrder;
            ReceiveOrderDetails = detail.ReceiveOrderDetails;
            ReturnedOrderDetails = detail.ReturnedOrderDetails;
            StockItem = detail.StockItem;

            StockItemDescription = detail.StockItem.Description;
            StockItemVendorPartNumber = detail.StockItem.VendorStockNumber;
        }


        public int TotalReceivedQuantity()
        {
            return ReceiveOrderDetails?.Sum(r => r.QuantityReceived) ?? 0;
        }

        public int QuantityOutstanding()
        {
            return Quantity - TotalReceivedQuantity();
        }
    }

}
