#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PurchaseOrderSystem.DAL;
using PurchaseOrderSystem;
using PurchaseOrderSystem.ViewModels;

namespace PurchaseOrderSystem.BLL
{
    public class InventoryService
    {
        private readonly eTools2023Context? _eTools2023Context;

        internal InventoryService(eTools2023Context eToolsContext)
        {
            _eTools2023Context = eToolsContext;
        }
        public List<ItemView> FetchInventoryBy(int vendorID)
        {
            return _eTools2023Context.StockItems
                .Where(x => x.VendorID == vendorID && x.Discontinued == false)
                .Select(x => new ItemView
                {
                    PurchaseOrderDetailID = 0,
                    StockItemID = x.StockItemID,
                    Description = x.Description,
                    QOH = x.QuantityOnHand,
                    ROL = x.ReOrderLevel,
                    QOO = x.QuantityOnOrder,
                    QTO = x.ReOrderLevel - x.QuantityOnHand - x.QuantityOnOrder >= 1 ? (x.ReOrderLevel - x.QuantityOnHand - x.QuantityOnOrder) : 0,
                    Price = x.PurchasePrice
                })
                .ToList();
        }

        public List<ItemView> FetchInventoryBy(PurchaseOrderView purchaseOrderInfo, int vendorID)
        {
            var items = purchaseOrderInfo.Items.Select(x=>x.StockItemID);
            if (purchaseOrderInfo.PurchaseOrderID != 0)
            {
                return _eTools2023Context.StockItems
            .Where(x => x.VendorID == vendorID)
            .Select(x => new ItemView
            {
                PurchaseOrderDetailID = _eTools2023Context.StockItems.Any(item => _eTools2023Context.PurchaseOrderDetails.Where(item => item.PurchaseOrderID == purchaseOrderInfo.PurchaseOrderID && x.StockItemID == item.StockItemID && x.RemoveFromViewFlag == false && x.Discontinued == false).Select(x => x.StockItemID).Contains(item.StockItemID)) == true ? 1 : 0,
                StockItemID = x.StockItemID,
                Description = x.Description,
                QOH = x.QuantityOnHand,
                ROL = x.ReOrderLevel,
                QOO = x.QuantityOnOrder,
                QTO = 0,
                Price = x.PurchasePrice
            }).Where(x => x.PurchaseOrderDetailID == 0).ToList();
            }
            else
            {
                return _eTools2023Context.StockItems
            .Where(x => x.VendorID == vendorID && !items.Any(s => s.Equals(x.StockItemID)))
            .Select(x => new ItemView
            {
                StockItemID = x.StockItemID,
                Description = x.Description,
                QOH = x.QuantityOnHand,
                ROL = x.ReOrderLevel,
                QOO = x.QuantityOnOrder,
                QTO = 0,
                Price = x.PurchasePrice
            }).ToList();
              
            }
        }
    }
}
