using Microsoft.AspNetCore.Components;
using RentalSystem.BLL;
using RentalSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CrusteaceanConglomerateETOOLS.Pages.RentalPages
{
    public partial class Rentals
    {
        private string phoneNumber;
        private string couponCode;
        private string feedbackMessage;
        private string errorMessage;
        private List<string> errorDetails = new();

        private CustomerView customer;
        private decimal discount;
        private RentalsView rentalsView = new RentalsView();
        private List<AvailableEquipmentView> availableEquipment = new List<AvailableEquipmentView>();
        private List<AvailableEquipmentView> rentalEquipment = new List<AvailableEquipmentView>();
        protected List<CustomerView> Customers { get; set; } = new();
        private DateTime currentRentalDate = DateTime.Now;

        [Inject]
        protected RentService RentService { get; set; }
        [Inject]
        protected CustomerService CustomerService { get; set; }

        private async void Search()
        {
            errorMessage = null;
            errorDetails.Clear();

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                errorDetails.Add("Please provide a phone number for the customer!");
            }
            else
            {
                customer = CustomerService.GetCustomerByPhone(phoneNumber);

                if (customer != null)
                {
                    availableEquipment = RentService.GetEquipments();
                    feedbackMessage = "Search for the customer was successful";
                }
                else
                {
                    errorDetails.Add("No customer was found for the provided phone number.");
                    availableEquipment = null;
                }
            }

            if (errorDetails.Any())
            {
                errorMessage = string.Join("\n", errorDetails);
            }

            await InvokeAsync(StateHasChanged);
        }

        private async void Validate()
        {
            discount = RentService.GetCoupon(couponCode);

            await InvokeAsync(StateHasChanged);
        }

        private async void AddToRental(AvailableEquipmentView equipment)
        {
            if (rentalsView.RentalDetails == null)
            {
                rentalsView.RentalDetails = new List<RentalDetailView>();
            }

            if (rentalsView.RentalDateOut == DateTime.MinValue)
            {
                rentalsView.RentalDateOut = currentRentalDate;
            }

            availableEquipment.Remove(equipment);
            rentalEquipment.Add(equipment);

            rentalsView.RentalDetails.Add(new RentalDetailView
            {
                RentalEquipmentID = equipment.RentalEquipmentID,
                RentalRate = equipment.DailyRate,
                RentalDays = 1,
                OutCondition = "out on rental",
                InCondition = "Good",
                DamageRepairCost = 0.0m,
                Comments = null
            });

            rentalsView.SubTotal += equipment.DailyRate;
            rentalsView.TaxAmount = CalculateTax(rentalsView.SubTotal);

            await InvokeAsync(StateHasChanged);
        }

        private async void RemoveFromRental(AvailableEquipmentView equipment)
        {
            rentalEquipment.Remove(equipment);
            availableEquipment.Add(equipment);

            var rentalDetailToRemove = rentalsView.RentalDetails
                .FirstOrDefault(rd => rd.RentalEquipmentID == equipment.RentalEquipmentID);

            if (rentalDetailToRemove != null)
            {
                rentalsView.SubTotal -= rentalDetailToRemove.RentalRate;
                rentalsView.TaxAmount = CalculateTax(rentalsView.SubTotal);
                rentalsView.RentalDetails.Remove(rentalDetailToRemove);
            }

            await InvokeAsync(StateHasChanged);
        }

        private async void Rent(RentalsView rental)
        {
            try
            {
                if (customer == null || rentalEquipment == null || rentalEquipment.Count == 0)
                {
                    errorMessage = "Customer and equipment are required for rental.";
                    return;
                }

                decimal discount = RentService.GetCoupon(couponCode);

                var rentalsView = new RentalsView
                {
                    EmployeeID = 1,
                    CustomerID = customer.CustomerID,
                    RentalDateOut = DateTime.Now,
                    RentalDateIn = DateTime.Now.AddDays(1),
                    PaymentType = "C",
                    RentalDetails = rentalEquipment.Select(equipment => new RentalDetailView
                    {
                        RentalEquipmentID = equipment.RentalEquipmentID,
                        RentalRate = equipment.DailyRate,
                        RentalDays = 1,
                        OutCondition = "Good",
                        InCondition = "Good"
                    }).ToList()
                };

                var rentedRental = RentService.Rent(rentalsView);

                if (rentedRental != null && rentedRental.RentalID > 0)
                {
                    feedbackMessage = "Rental was successful. Rental ID: " + rentedRental.RentalID;
                    customer = null;
                    rentalEquipment.Clear();
                }
                else
                {
                    errorMessage = "Failed to add rental to the database. Please try again.";
                }
            }
            catch (Exception ex)
            {
                errorMessage = "An error occurred while processing the rental.";
                Console.WriteLine(ex.Message);
            }

            await InvokeAsync(StateHasChanged);
        }

        private decimal CalculateTax(decimal subTotal)
        {
            const decimal taxRate = 0.1m;
            return subTotal * taxRate;
        }
    }
}



