using Microsoft.Identity.Client;
using SaleSystem.DAL;
using SaleSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaleSystem.BLL
{
    public class SaleService
    {
        #region
        private readonly eTools2023Context? _saleContext;
        #endregion

        internal SaleService (eTools2023Context saleContext)
        {
            _saleContext = saleContext;
        }

        public List<CategoryView> GetCategories()
        {
            return _saleContext.Categories
                .Select(x => new CategoryView
                {
                    CategoryID = x.CategoryID,
                    Description = x.Description,
                    StockItemCount = x.StockItems.Count
                })
                .ToList();
        }

        public List<StockItemView> GetItemsByCategoryID(int categoryID)
        {


            if (categoryID < 0)
            {
                throw new ArgumentNullException("Invalid Category ID");
            }

            return _saleContext.StockItems
                .Where(x => x.CategoryID == categoryID)
                .Select(x => new StockItemView
                {
                    StockItemID = x.StockItemID,
                    SellingPrice = x.SellingPrice,
                    Description = x.Description,
                    QuantityOnHand = x.QuantityOnHand,
                    //QuantityOnOrder = x.QuantityOnOrder,
                })
                .ToList();
        }

        public ShoppingCartView GetCart(int stockID)
        {

            return _saleContext.StockItems
                .Where(x => x.StockItemID == stockID)
                .Select(x => new ShoppingCartView
                {
                    StockItemID = x.StockItemID,
                    SellingPrice = x.SellingPrice,
                    Description = x.Description,
                    Quantity = x.QuantityOnOrder,
                    QuantityOnHand = x.QuantityOnHand
                })
                .FirstOrDefault();
        }

        public int SaveSales(SaleView sale)
        {
            List<Exception> errorList = new List<Exception>();

            #region Business Logic
            if (sale.Items.Count == 0)
            {
                errorList.Add(new Exception("No items currently in cart"));
            }

            if (sale.SaleID < 0)
            {
                errorList.Add(new Exception("Invalid Sale ID"));
            }

            if (sale.EmployeeID < 0)
            {
                errorList.Add(new Exception("Invalid Employee ID"));
            }

            if (sale.PaymentType != "M" && sale.PaymentType != "C" && sale.PaymentType != "D")
            {
                errorList.Add(new Exception("Payment Type can only be 'M' (Money) 'C' (Credit) or 'D' (Debit)"));
            }

            if (sale.TaxAmount < 0)
            {
                errorList.Add(new Exception("TaxAmount cannot be negative"));
            }

            if (sale.SubTotal < 0)
            {
                errorList.Add(new Exception("SubTotal cannot be negative"));
            }

            if (sale.CouponID < 0)
            {
                errorList.Add(new Exception("Coupon ID cannot be negative"));
            }
            #endregion


            SaleView newSale = new();
            newSale.SaleID = sale.SaleID;
            newSale.EmployeeID = sale.EmployeeID;
            newSale.PaymentType = sale.PaymentType;
            newSale.TaxAmount = sale.TaxAmount;
            newSale.SubTotal = sale.SubTotal;
            newSale.CouponID = sale.CouponID;
            newSale.Items = sale.Items;

            if (errorList.Count > 0)
            {
                _saleContext.ChangeTracker.Clear();
                Console.WriteLine("Cannot process sale due to:");
                throw new AggregateException("Unable to proceed.", errorList);
            }
            else
            {
                _saleContext.SaveChanges();
            }

            return newSale.SaleID;
        }


    }
}
