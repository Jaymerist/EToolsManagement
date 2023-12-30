<Query Kind="Program">
  <Connection>
    <ID>1e97515d-9fc0-4e1f-82f8-bca09e803ad1</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Driver Assembly="(internal)" PublicKeyToken="no-strong-name">LINQPad.Drivers.EFCore.DynamicDriver</Driver>
    <Server>.</Server>
    <Database>eTools2023</Database>
    <DisplayName>eTools2023-Entity</DisplayName>
    <DriverData>
      <EncryptSqlTraffic>False</EncryptSqlTraffic>
      <PreserveNumeric1>True</PreserveNumeric1>
      <EFProvider>Microsoft.EntityFrameworkCore.SqlServer</EFProvider>
      <TrustServerCertificate>False</TrustServerCertificate>
    </DriverData>
  </Connection>
</Query>

void Main()
{
	try
	{
		//YOUR CODE HERE
		
		//Query Methods
		//GetCustomerByPhone("589.428.6764").Dump();
		//GetEquipments().Dump();
		//GetCoupon("Joy23").Dump();
		//GetRentalByRentalNumber(4).Dump();
		//GetRentalByPhone("589.428.6764").Dump();
		//SelectRentalByCustomer(35).Dump();
		
		//Driver
		
	}
	catch (ArgumentNullException ex)
	{
		GetInnerException(ex).Message.Dump();
	}
	catch (ArgumentException ex)
	{

		GetInnerException(ex).Message.Dump();
	}
	catch (AggregateException ex)
	{
		//having collected a number of errors
		//	each error should be dumped to a separate line
		foreach (var error in ex.InnerExceptions)
		{
			error.Message.Dump();
		}
	}
	catch (Exception ex)
	{
		GetInnerException(ex).Message.Dump();
	}
}


#region Methods
private Exception GetInnerException(Exception ex)
{
	while (ex.InnerException != null)
		ex = ex.InnerException;
	return ex;
}
// RENTAL SECTION
public CustomerView GetCustomerByPhone(string phone)
{
	return Customers
		.Where(x => x.ContactPhone == phone)
		.Select(x => new CustomerView
		{
			CustomerID = x.CustomerID,
			LastName = x.LastName,
			FirstName = x.FirstName,
			Address = x.Address,
		}).FirstOrDefault();
}

public List<AvailableEquipmentView> GetEquipments()
{
	return RentalEquipments
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
	return Coupons
		.Where(x => x.CouponIDValue == coupon)
		.Select(x => x.CouponDiscount).FirstOrDefault();
}

public RentalsView Rent(RentalsView rental)
{
	List<Exception> errorlist = new List<Exception>();

	#region Business Rules

	if (rental.EmployeeID == 0)
	{
		throw new Exception("An employee must be logged in to rent equipment.");
	}
	
	if(rental.CustomerID == 0)
	{
		throw new Exception("Customer record is required!");
	}
	

	if (rental.RentalDateOut == null || rental.RentalDateOut == DateTime.MinValue)
	{
		throw new Exception("Rental date out must be set.");
	}

	if (rental.RentalDetails == null || rental.RentalDetails.Count == 0)
	{
		throw new Exception("Cannot process rental without rental equipment.");
	}
	
	rental.RentalDateIn = rental.RentalDateIn == null || rental.RentalDateIn == DateTime.MinValue
		 ? DateTime.Now.AddDays(1) // Default to next day if not set
		 : rental.RentalDateIn;

	rental.PaymentType = string.IsNullOrEmpty(rental.PaymentType) ? "Credit Card" : rental.PaymentType;
	rental.SubTotal = 0.0m;
	rental.TaxAmount = 0.0m;


	foreach (var rentalDetail in rental.RentalDetails)
    {
        if (rentalDetail.RentalDays < 0.5m)
        {
            throw new Exception("The smallest rental period offered is a half-day.");
        }

        var equipment = RentalEquipments.FirstOrDefault(e => e.RentalEquipmentID == rentalDetail.RentalEquipmentID);

        if (equipment == null)
        {
            throw new Exception($"Rental equipment with ID {rentalDetail.RentalEquipmentID} not found.");
        }

        if (!equipment.Available)
        {
            throw new Exception($"This rental equipment is unavailable");
        }

        equipment.Available = false;

        if (!Customers.Any(c => c.CustomerID == rental.CustomerID && c.AcceptableStatus))
        {
            throw new Exception("Customer does not have a good status to rent equipment.");
        }

        rentalDetail.OutCondition = "Out on Rental";
        rentalDetail.InCondition = null;
        rentalDetail.DamageRepairCost = 0.0m;
        rentalDetail.Comments = null;

		rental.SubTotal += rentalDetail.RentalRate * rentalDetail.RentalDays;
	}

	
	
	var distinctRentalEquipmentIDs = rental.RentalDetails.Select(rd => rd.RentalEquipmentID).Distinct().ToList();
	if (distinctRentalEquipmentIDs.Count != rental.RentalDetails.Count)
	{
		throw new Exception("Equipment rentals must be based on full sets, and no partial returns are allowed.");
	}

	
	var distinctRentalDays = rental.RentalDetails.Select(rd => rd.RentalDays).Distinct().ToList();
	if (distinctRentalDays.Count > 1)
	{
		throw new Exception("Separate contracts are required for equipment with different rental periods.");
	}

	if (errorlist.Count > 0)
	{
		ChangeTracker.Clear();
		string errorMsg = "Please check error message(s)";
		throw new AggregateException(errorMsg, errorlist);
	}
	else
	{
		SaveChanges();
	}

	#endregion

	return rental;
}


// RETURN SECTION
public RentalsView GetRentalByRentalNumber(int rentalid)
{
	return Rentals
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
	return Rentals
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

public List<RentalsView> SelectRentalByCustomer(int customerID)
{
	return Rentals
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


public RentalsView Return(RentalsView rental)
{
	List<Exception> errorlist = new List<Exception>();

	#region Business Rules

	foreach (var rentalDetail in rental.RentalDetails)
	{
		var equipment = RentalEquipments.FirstOrDefault(e => e.RentalEquipmentID == rentalDetail.RentalEquipmentID);

		if (equipment == null)
		{
			throw new Exception($"Rental equipment with ID {rentalDetail.RentalEquipmentID} not found.");
		}

		if (rentalDetail.InCondition == "Good")
		{
			equipment.Available = true;
		}
		else
		{
			equipment.Available = false;
		}

		
		equipment.Condition = rentalDetail.InCondition;

		
		var validPaymentTypes = new List<string> { "Money", "Credit", "Debit" };
		if (!validPaymentTypes.Contains(rental.PaymentType))
		{
			throw new Exception("Payment types must include 'Money', 'Credit', and 'Debit'.");
		}

		rental.TaxAmount = rental.SubTotal * 0.1m;
		rental.RentalDateIn = DateTime.Now;

		if (errorlist.Count() > 0)
		{
			ChangeTracker.Clear();
			throw new AggregateException("Unable to proceed.", errorlist);
		}
		else
		{
			SaveChanges();
		}
	}

	#endregion

	return rental;
}

#region Class/View Models
public class CustomerView
{
	public int CustomerID {get; set;}
	public string LastName {get; set;}
	public string FirstName {get; set;}
	public string Address {get; set;}
	public int RentalID {get; set;}
}

public class AvailableEquipmentView
{
	public int RentalEquipmentID {get; set;}
	public string Description {get; set;}
	public string SerialNumber {get; set;}
	public decimal DailyRate {get; set;}
	public string Condition {get; set;}
}

public class RentalsView
{
	public int RentalID {get; set;}
	public int CustomerID {get; set;}
	public int EmployeeID {get; set;}
	public int? CouponID {get; set;}
	public decimal SubTotal {get; set;}
	public decimal TaxAmount {get; set;}
	public DateTime RentalDateOut {get; set;}
	public DateTime RentalDateIn {get; set;}
	public string PaymentType {get; set;}
	public List<RentalDetailView> RentalDetails {get; set;}
}

public class RentalDetailView
{
	public int RentalDetailID {get; set;}
	public int RentalID {get; set;}
	public int RentalEquipmentID {get; set;}
	public decimal RentalDays {get; set;}
	public decimal RentalRate {get; set;}
	public string OutCondition {get; set;}
	public string InCondition {get; set;}
	public decimal DamageRepairCost {get; set;}
	public string Comments {get; set;}
}
#endregion

// You can define other methods, fields, classes and namespaces here