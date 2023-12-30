#nullable disable
using RentalSystem.DAL;
using RentalSystem.ViewModels;
using RentalSystem.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;

namespace RentalSystem.BLL
{
    public class RentService
    {
        private readonly eTools2023Context _eTools2023Context;

        internal RentService(eTools2023Context eToolsContext)
        {
            _eTools2023Context = eToolsContext;
        }

        public List<AvailableEquipmentView> GetEquipments()
        {
            return _eTools2023Context.RentalEquipments
                .OrderBy(x => x.Description)
                .Select(x => new AvailableEquipmentView
                {
                    RentalEquipmentID = x.RentalEquipmentID,
                    Description = x.Description,
                    SerialNumber = x.SerialNumber,
                    DailyRate = x.DailyRate,
                    Condition = x.Condition
                }).ToList();
        }

        public decimal GetCoupon(string coupon)
        {
            return _eTools2023Context.Coupons
                .Where(x => x.CouponIDValue == coupon)
                .Select(x => x.CouponDiscount)
                .FirstOrDefault();
        }

        public Rental Rent(RentalsView rentalView)
        {
            List<Exception> errorList = new List<Exception>();

            var rental = new Rental
            {
                EmployeeID = rentalView.EmployeeID,
                CustomerID = rentalView.CustomerID,
                RentalDateOut = rentalView.RentalDateOut,
                RentalDateIn = rentalView.RentalDateIn,
                PaymentType = rentalView.PaymentType,
                SubTotal = rentalView.SubTotal,
                TaxAmount = rentalView.TaxAmount
            };

            foreach (var rentalDetail in rentalView.RentalDetails)
            {
                var rentalDetailEntity = new RentalDetail
                {
                    RentalEquipmentID = rentalDetail.RentalEquipmentID,
                    RentalRate = rentalDetail.RentalRate,
                    RentalDays = rentalDetail.RentalDays,
                    OutCondition = rentalDetail.OutCondition,
                    InCondition = rentalDetail.InCondition,
                    DamageRepairCost = rentalDetail.DamageRepairCost,
                    Comments = rentalDetail.Comments
                };

                rental.RentalDetails.Add(rentalDetailEntity);
            }

            try
            {
                _eTools2023Context.Rentals.Add(rental);
                _eTools2023Context.SaveChanges();
            }
            catch (Exception ex)
            {
                errorList.Add(new Exception("An error occurred while processing the rental.", ex));
                Console.WriteLine($"Exception: {ex.Message}");
            }

            if (errorList.Count > 0)
            {
                _eTools2023Context.ChangeTracker.Clear();
                Console.WriteLine("Cannot process rental due to:");
                throw new AggregateException("Unable to proceed with the rental.", errorList);
            }
            else
            {
                return rental;
            }
        }
    }
}


