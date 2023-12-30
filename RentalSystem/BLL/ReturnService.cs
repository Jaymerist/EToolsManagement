#nullable disable
using RentalSystem.DAL;
using RentalSystem.ViewModels;
using RentalSystem.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace RentalSystem.BLL
{
    public class ReturnService
    {
        private readonly eTools2023Context? _eTools2023Context;
        internal ReturnService(eTools2023Context eToolsContext)
        {
            _eTools2023Context = eToolsContext;
        }

        public RentalsView GetRentalByRentalNumber(int rentalid)
        {
            return _eTools2023Context.Rentals
                .Where(x => x.RentalID == rentalid)
                .Select(x => new RentalsView
                {
                    RentalID = x.RentalID,
                    CustomerID = x.CustomerID,
                    EmployeeID = x.EmployeeID,
                    CouponID = x.Coupon.CouponID,
                    SubTotal = x.SubTotal,
                    TaxAmount = x.TaxAmount,
                    RentalDateOut = x.RentalDateOut,
                    RentalDateIn = x.RentalDateIn,
                    PaymentType = x.PaymentType,
                    RentalDetails = x.RentalDetails
                                        .Where(r => r.RentalID == rentalid)
                                        .Select(r => new RentalDetailView
                                        {
                                            RentalDetailID = r.RentalDetailID,
                                            RentalID = r.RentalID,
                                            RentalEquipmentID = r.RentalEquipmentID,
                                            RentalDays = r.RentalDays,
                                            RentalRate = r.RentalRate,
                                            OutCondition = r.OutCondition,
                                            InCondition = r.InCondition,
                                            DamageRepairCost = r.DamageRepairCost
                                        }).ToList()
                }).FirstOrDefault();
        }

        public List<RentalsView> GetRentalByPhone(string phone)
        {
            return _eTools2023Context.Rentals
                .Where(x => x.Customer.ContactPhone == phone)
                .Select(x => new RentalsView
                {
                    RentalID = x.RentalID,
                    CustomerID = x.CustomerID,
                    EmployeeID = x.EmployeeID,
                    CouponID = x.Coupon.CouponID,
                    SubTotal = x.SubTotal,
                    TaxAmount = x.TaxAmount,
                    RentalDateOut = x.RentalDateOut,
                    RentalDateIn = x.RentalDateIn,
                    PaymentType = x.PaymentType,
                    RentalDetails = x.RentalDetails
                                        .Where(r => r.Rental.Customer.ContactPhone == phone)
                                        .Select(r => new RentalDetailView
                                        {
                                            RentalDetailID = r.RentalDetailID,
                                            RentalID = r.RentalID,
                                            RentalEquipmentID = r.RentalEquipmentID,
                                            RentalDays = r.RentalDays,
                                            RentalRate = r.RentalRate,
                                            OutCondition = r.OutCondition,
                                            InCondition = r.InCondition,
                                            DamageRepairCost = r.DamageRepairCost
                                        }).ToList()
                }).ToList();
        }

        public RentalsView Return(RentalsView rental)
        {
            List<Exception> errorlist = new List<Exception>();

            #region Business Rules
                var validPaymentTypes = new List<string> { "Money", "Credit", "Debit" };
                if (!validPaymentTypes.Contains(rental.PaymentType))
                {
                    throw new Exception("Payment types must include 'Money', 'Credit', and 'Debit'.");
                }

                rental.TaxAmount = rental.SubTotal * 0.1m;
                rental.RentalDateIn = DateTime.Now;

                if (errorlist.Count() > 0)
                {
                    _eTools2023Context.ChangeTracker.Clear();
                    throw new AggregateException("Unable to proceed.", errorlist);
                }
                else
                {
                    _eTools2023Context.SaveChanges();
                }
            

            #endregion

            return rental;
        }
    }
}
