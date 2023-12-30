#nullable disable
using RentalSystem.DAL;
using RentalSystem.ViewModels;
using RentalSystem.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace RentalSystem.BLL
{
    public class CustomerService
    {
        private readonly eTools2023Context? _eTools2023Context;
        internal CustomerService(eTools2023Context eToolsContext)
        {
            _eTools2023Context = eToolsContext;
        }
        public CustomerView GetCustomerByPhone(string phone)
        { 
            return _eTools2023Context.Customers
                .Where(x => x.ContactPhone == phone)
                .Select(x => new CustomerView
                {
                    CustomerID = x.CustomerID,
                    LastName = x.LastName,
                    FirstName = x.FirstName,
                    Address = x.Address,
                }).FirstOrDefault();
        }

        public List<RentalsView> SelectRentalByCustomer(int customerID)
        {
            return _eTools2023Context.Rentals
                .Where(x => x.CustomerID == customerID)
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
                                        .Where(r => r.Rental.CustomerID == customerID)
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
    }
}
