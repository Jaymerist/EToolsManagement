using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SaleSystem.DAL;
using SaleSystem.ViewModels;

namespace SaleSystem.BLL
{
    public class RefundService
    {
        #region
        private readonly eTools2023Context? _refundContext;
        #endregion

        internal RefundService(eTools2023Context refundContext)
        {
            _refundContext = refundContext;
        }

        public int GetCoupon(string couponName)
        {
            List<Exception> errorList = new List<Exception>();
            
            // Temporary
            var couponOneStartDate = new DateTime(2017, 11, 23, 12, 00, 00);
            var couponOneEndDate = new DateTime(2017, 11, 25, 12, 00, 00);
            var couponTwoStartDate = new DateTime(2017, 12, 01, 12, 00, 00);
            var couponTwoEndDate = new DateTime(2017, 12, 24, 12, 00, 00);
            var couponThreeStartDate = new DateTime(2017, 12, 26, 12, 00, 00);
            var couponThreeEndDate = new DateTime(2017, 12, 31, 12, 00, 00); // Database says 2011
            var couponFourStartDate = new DateTime(2018, 04, 10, 12, 00, 00);
            var couponFourEndDate = new DateTime(2018, 04, 25, 12, 00, 00);
            DateTime currentDate = DateTime.Now;

            CouponView coupon = new();

            string couponOne = "Joy23";
            string couponTwo = "HandsOn";
            string couponThree = "NewStarts";
            string couponFour = "SpringJump";


            if (coupon.CouponID != 4 && coupon.CouponID != 14 && coupon.CouponID != 15 && coupon.CouponID != 18)
            {
                throw new ArgumentNullException("The Coupon ID Provided is invalid");
            }
            
            if (couponName.Equals(couponOne) || couponName.Equals(couponTwo) || couponName.Equals(couponThree) || couponName.Equals(couponFour))
            {
                Console.WriteLine("Successfully added coupon");
            }
            else
            {
                throw new ArgumentException("Coupon doesn't exist");
            }

            if (currentDate < couponOneStartDate)
            {
                errorList.Add(new Exception($"Coupon: '{couponOne}' cannot be used yet"));
            }
            else if (currentDate > couponOneEndDate)
            {
                errorList.Add(new Exception($"Coupon: '{couponOne}' is expired"));
            }

            if (currentDate < couponTwoStartDate)
            {
                errorList.Add(new Exception($"Coupon: '{couponTwo}' cannot be used yet"));
            }
            else if (currentDate > couponTwoEndDate)
            {
                errorList.Add(new Exception($"Coupon: '{couponTwo}' is expired"));
            }

            if (currentDate < couponThreeStartDate)
            {
                errorList.Add(new Exception($"Coupon: '{couponThree}' cannot be used yet"));
            }
            else if (currentDate > couponThreeEndDate)
            {
                errorList.Add(new Exception($"Coupon: '{couponThree}' is expired"));
            }

            if (currentDate < couponFourStartDate)
            {
                errorList.Add(new Exception($"Coupon: '{couponFour}' cannot be used yet"));
            }
            else if (currentDate > couponFourEndDate)
            {
                errorList.Add(new Exception($"Coupon: '{couponFour}' is expired"));
            }

            CouponView newCoupon = new();
            newCoupon.CouponID = coupon.CouponID;
            newCoupon.CouponValue = coupon.CouponValue;
            newCoupon.Discount = coupon.Discount;
            newCoupon.StartDate = coupon.StartDate;
            newCoupon.EndDate = coupon.EndDate;

            if (errorList.Count > 0)
            {
                _refundContext.ChangeTracker.Clear();
                Console.WriteLine("Cannot process sale due to:");
                throw new AggregateException("Unable to proceed.", errorList);
            }
            else
            {
               _refundContext.SaveChanges();
            }

            return coupon.CouponID;
        }

        public ReturnSaleView GetSaleRefund(int saleID, SaleView sale)
        {
            if (saleID != sale.SaleID)
            {
                throw new ArgumentException("Invalid Sale ID");
            }

            return _refundContext.SaleRefunds
                .Select(x => new ReturnSaleView
                {
                    SaleID = x.SaleID,
                    EmployeeID = x.EmployeeID,
                    TaxAmount = x.TaxAmount,
                    SubTotal = x.SubTotal,
                    CouponID = x.Sale.CouponID ?? 0,
                    ReturnSaleDetails = x.SaleRefundDetails
                                            .Select(c => new ReturnSaleDetailCartView
                                            {
                                                StockItemID = c.StockItemID,
                                                Description = c.StockItem.Description,
                                                OriginalQty = c.Quantity,
                                                SellingPrice = c.SellingPrice,
                                                PreviousReturnQty = c.StockItem.SaleRefundDetails.Count(),
                                                QtyReturnNow = c.StockItem.QuantityOnOrder
                                            })
                                            .ToList()
                })
                .FirstOrDefault();
        }

        public int SaveRefund(ReturnSaleView refundSale, SaleView sale)
        {
            List<Exception> errorList = new List<Exception>();


            if (refundSale.SaleID != sale.SaleID)
            {
                errorList.Add(new Exception("Original Sale ID does not match"));
            }

            if (string.IsNullOrWhiteSpace(refundSale.Reason))
            {
                errorList.Add(new Exception("Refunds must have Reason"));
            }

            ReturnSaleView newReturn = new();
            newReturn.SaleID = refundSale.SaleID;
            newReturn.CouponID = refundSale.CouponID;
            newReturn.Reason = refundSale.Reason;
            newReturn.EmployeeID = refundSale.EmployeeID;
            newReturn.SubTotal = refundSale.SubTotal;
            newReturn.TaxAmount = refundSale.TaxAmount;

            if (errorList.Count > 0)
            {
                _refundContext.ChangeTracker.Clear();
                Console.WriteLine("Cannot process sale due to:");
                throw new AggregateException("Unable to proceed.", errorList);
            }
            else
            {
                _refundContext.SaveChanges();
            }

            return newReturn.SaleID;
        }
    }
}
