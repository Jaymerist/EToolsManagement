<Query Kind="Program">
  <Connection>
    <ID>29b5dcfc-9796-468d-a059-a56afecc98fd</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Driver Assembly="(internal)" PublicKeyToken="no-strong-name">LINQPad.Drivers.EFCore.DynamicDriver</Driver>
    <Server>LAPTOP-294TPGP1</Server>
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
	#region Driver
	try
	{
		// Calling Query Methods
		//GetCategory().Dump();
		//GetItemsByCategoryID(5).Dump();
		//GetSaleRefund(1).Dump();
		
		#region Init
			ShoppingCartView shoppingCart = new ShoppingCartView();
			ShoppingCartView shoppingCartTwo = new ShoppingCartView();
			StockItemsView itemStock = new StockItemsView();
			SaleView addSales = new SaleView();
			ReturnSalesView saleRefund = new ReturnSalesView();
			ReturnSalesDetailCartView refundedItem = new ReturnSalesDetailCartView();

			// Initializing Lists
			saleRefund.ReturnSalesDetails = saleRefund.ReturnSalesDetails ?? new List<ReturnSalesDetailCartView>();
			addSales.Items = addSales.Items ?? new List<ShoppingCartView>();
		#endregion

		#region Sales
			// Testing Business Rules
			// Good Data
			addSales.SaleID = 1;
			addSales.EmployeeID = 2;
			addSales.PaymentType = "M";
			addSales.CouponID = 0;
			// Adding item to cart
			addSales.Items.Add(shoppingCart);
			shoppingCart.StockItemID = 123;
			shoppingCart.Description = "Screwdriver";
			shoppingCart.Quantity = 2;
			shoppingCart.SellingPrice = 5.00m * shoppingCart.Quantity;
			shoppingCart.Dump("Current Items in Shopping Cart");

			// Adding another item to the shopping cart
			addSales.Items.Add(shoppingCartTwo);
			// Change StockItemID to 123 or the same as any other StockItemID to test business rule for duplicates
			shoppingCartTwo.StockItemID = 214;
			shoppingCartTwo.Description = "Ratchet";
			shoppingCartTwo.Quantity = 1;
			shoppingCartTwo.SellingPrice = 10.00m * shoppingCartTwo.Quantity;
			shoppingCartTwo.Dump("-");
			
			//To simulate a transaction
			//Thread.Sleep(4000);
			
			// Tax and SubTotal Calculations
			addSales.TaxAmount = (shoppingCart.SellingPrice + shoppingCartTwo.SellingPrice) * 0.05m;
			addSales.SubTotal = addSales.TaxAmount + (shoppingCart.SellingPrice + shoppingCartTwo.SellingPrice);
			
			// Updating Quantity
			//addSales.Items[0].Quantity = 3;
			
			// Returns SaleID
			SaveSales(addSales, shoppingCart).Dump("Successfully added to cart (SaleID):");
			addSales.Dump("Sale");
			
			// Bad Data for Sales
			//addSales.SaleID = -1;
			//addSales.EmployeeID = -2;
			//addSales.PaymentType = "DM";
			//addSales.TaxAmount = -4.00m;
			//addSales.SubTotal = -14.00m;
			//addSales.Discontinued = true;
			//addSales.CouponID = -1;
			//// Adding "Ratchet" item to cart
			//addSales.Items.Add(shoppingCart);
			//shoppingCart.StockItemID = -123;
			//shoppingCart.Description = null;
			//shoppingCart.Quantity = -1;
			//shoppingCart.SellingPrice = -10.00m;
			//SaveSales(addSales, shoppingCart).Dump("Successfully added Sale ID:");
			//addSales.Dump("Order");
		#endregion

		#region Refunded Sales
			// Refunded Sales
			// Good Data
			saleRefund.SaleID = addSales.SaleID;
			saleRefund.EmployeeID = addSales.EmployeeID;
			saleRefund.TaxAmount = addSales.TaxAmount;
			saleRefund.SubTotal = addSales.SubTotal;
			saleRefund.CouponID = addSales.CouponID;
			saleRefund.Reason = "Wrong Item";
			saleRefund.ReturnSalesDetails.Add(refundedItem);
			refundedItem.StockItemID = shoppingCart.StockItemID;
			refundedItem.Description = shoppingCart.Description;
			refundedItem.OriginalQty = shoppingCart.Quantity;
			refundedItem.SellingPrice = shoppingCart.SellingPrice;
			// Assuming that this is the amouunt of items refunded
			refundedItem.PreviousReturnQty = 1;
			refundedItem.QtyReturnNow = shoppingCart.Quantity - refundedItem.PreviousReturnQty;
			
			// Returns SaleID
			SaveRefunds(saleRefund, addSales, shoppingCart, refundedItem).Dump("Successfully refunded (SaleID):");
			saleRefund.Dump("Refunded Item(s)");
		
			// Bad Data for Refunded Sales
			//saleRefund.SaleID = 4;
			//saleRefund.EmployeeID = -4;
			//saleRefund.TaxAmount = -4;
			//saleRefund.SubTotal = -4;
			//refundedItem.StockItemID = -4;
			//refundedItem.Description = null;
			//refundedItem.OriginalQty = -4;
			//refundedItem.SellingPrice = -4;
			//refundedItem.PreviousReturnQty = -4;
			//refundedItem.QtyReturnNow = -4;
			//SaveRefunds(saleRefund, addSales, shoppingCart, refundedItem).Dump("Successfully refunded (SaleID):");
		#endregion
		
	}
	#region catch all exceptions 
	catch (AggregateException ex)
	{
		foreach (var error in ex.InnerExceptions)
		{
			error.Message.Dump();
		}
	}
	catch (ArgumentNullException ex)
	{
		GetInnerException(ex).Message.Dump();
	}
	catch (Exception ex)
	{
		GetInnerException(ex).Message.Dump();
	}
	#endregion
	#endregion
}

private Exception GetInnerException(Exception ex)
{
	while (ex.InnerException != null)
		ex = ex.InnerException;
	return ex;
}

#region Main Method

public int SaveSales(SaleView sale, ShoppingCartView cart) // added cart to test business rule
{
	List<Exception> errorList = new List<Exception>();
		
	if (sale.Items.Count() == 0)
	{
		// Not a business rule but wanted to show no items in cart.
		errorList.Add(new Exception("No items currently in cart"));
	}
	
	if (sale.SaleID < 0)
	{
		errorList.Add(new Exception("Must have a valid Sale ID"));
	}
	
	if (sale.EmployeeID < 0)
	{
		errorList.Add(new Exception("Must have a valid Employee ID"));
	}
	
	if (sale.PaymentType != "M" && sale.PaymentType != "C" && sale.PaymentType != "D" )
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
	
	if (sale.Discontinued == true)
	{
		errorList.Add(new Exception("Discontinued items cannot be added to the cart"));
	}

	if(cart.StockItemID < 0)
	{
		errorList.Add(new Exception("Must have a valid Stock Item ID"));
	}
	
	if (string.IsNullOrWhiteSpace(cart.Description))
	{
		errorList.Add(new Exception("Description cannot be empty"));
	}
	
	if (cart.Quantity <= 0)
	{
		errorList.Add(new Exception("Quantity cannot be 0 or negative"));
	}
	
	if (cart.SellingPrice < 0.00m)
	{
		errorList.Add(new Exception("Selling Price cannot be negative"));
	}
	
	if (sale.Items.GroupBy(item => item.StockItemID).Any(group => group.Count() > 1))
	{
		errorList.Add(new Exception("Duplicate items are not allowed in the sale"));
	}

	SaleView newSales = new();
	newSales.SaleID = sale.SaleID;
	newSales.EmployeeID = sale.EmployeeID;
	newSales.PaymentType = sale.PaymentType;
	newSales.TaxAmount = sale.TaxAmount;
	newSales.SubTotal = sale.SubTotal;
	newSales.CouponID = sale.CouponID;
	newSales.Discontinued = sale.Discontinued;
	newSales.Items = sale.Items;
	
	ShoppingCartView newCart = new();
	newCart.StockItemID = cart.StockItemID;
	newCart.Description = cart.Description;
	newCart.Quantity = cart.Quantity;
	newCart.SellingPrice = cart.SellingPrice;
	
	if (errorList.Count() > 0)
	{
		ChangeTracker.Clear();
		Console.WriteLine("Cannot process sale due to:");
		throw new AggregateException("Unable to proceed.", errorList);
	}
	else
	{
		SaveChanges();
	}
	
	return sale.SaleID;
}

public int SaveRefunds(ReturnSalesView refundSale, SaleView sale, ShoppingCartView cart, ReturnSalesDetailCartView returnCart) // added to test business rules
{
	List<Exception> errorList = new List<Exception>();
	
	if (refundSale.SaleID != sale.SaleID)
	{
		errorList.Add(new Exception("Original receipt required"));
	} else if (refundSale.SaleID < 0)
	{
		errorList.Add(new Exception("Must be a valid Sale ID"));
	}
	
	if (string.IsNullOrWhiteSpace(refundSale.Reason))
	{
		errorList.Add(new Exception("Reason required for return"));
	}
	
	if (refundSale.EmployeeID < 0)
	{
		errorList.Add(new Exception("Must be a valid Employee ID"));
	}
	
	if (refundSale.SubTotal < 0)
	{
		errorList.Add(new Exception("SubTotal cannot be negative"));
	}
	
	if (refundSale.CouponID < 0)
	{
		errorList.Add(new Exception("Coupoun ID cannot be a negative"));
	}
	
	if (returnCart.StockItemID != cart.StockItemID || returnCart.StockItemID < 0)
	{
		errorList.Add(new Exception("Item doesn't match original Stock Item ID"));
	}
	
	if (string.IsNullOrWhiteSpace(returnCart.Description))
	{
		errorList.Add(new Exception("Description cannot be empty or null"));
	}
	
	if (returnCart.OriginalQty != cart.Quantity || returnCart.OriginalQty < 0)
	{
		errorList.Add(new Exception("Quantity doesn't match original Quantity"));
	}
	
	if (returnCart.SellingPrice != cart.SellingPrice || returnCart.SellingPrice < 0)
	{
		errorList.Add(new Exception("Selling Price doesn't match orignal Selling Price"));
	}
	
	if (returnCart.PreviousReturnQty < 0)
	{
		errorList.Add(new Exception("Previous Return Quantity cannot be negative"));
	}
	
	if (returnCart.QtyReturnNow < 0)
	{
		errorList.Add(new Exception("Quantity Return Now cannot be negative"));
	}
	
	ReturnSalesView newReturn = new();
	newReturn.SaleID = refundSale.SaleID;
	newReturn.EmployeeID = refundSale.EmployeeID;
	newReturn.TaxAmount = refundSale.TaxAmount;
	newReturn.SubTotal = refundSale.SubTotal;
	newReturn.CouponID = refundSale.CouponID;
	newReturn.ReturnSalesDetails = refundSale.ReturnSalesDetails;

	SaleView newSales = new();
	newSales.SaleID = sale.SaleID;
	newSales.EmployeeID = sale.EmployeeID;
	newSales.PaymentType = sale.PaymentType;
	newSales.TaxAmount = sale.TaxAmount;
	newSales.SubTotal = sale.SubTotal;
	newSales.CouponID = sale.CouponID;

	ShoppingCartView newCart = new();
	newCart.StockItemID = cart.StockItemID;
	newCart.Description = cart.Description;
	newCart.Quantity = cart.Quantity;
	newCart.SellingPrice = cart.SellingPrice;
	
	ReturnSalesDetailCartView newReturnCart = new();
	newReturnCart.StockItemID = returnCart.StockItemID;
	newReturnCart.Description = returnCart.Description;
	newReturnCart.OriginalQty = returnCart.OriginalQty;
	newReturnCart.SellingPrice = returnCart.SellingPrice;
	newReturnCart.PreviousReturnQty = returnCart.PreviousReturnQty;
	newReturnCart.QtyReturnNow = returnCart.QtyReturnNow;

	if (errorList.Count() > 0)
	{
		ChangeTracker.Clear();
		Console.WriteLine("Cannot refund item due to:");
		throw new AggregateException("Unable to proceed.", errorList);
	}
	else
	{
		SaveChanges();
	}

	return refundSale.SaleID;
}


#endregion

#region Sales Queries

public List<CategoryView> GetCategory()
{
	return Categories
		.Select(x => new CategoryView
		{
			CategoryID = x.CategoryID,
			Description = x.Description,
			StockItemCount = x.StockItems.Count()
		})
		.ToList();
}

public List<StockItemsView> GetItemsByCategoryID(int categoryID)
{
	return StockItems
		.Where(x => x.CategoryID == categoryID)
		.Select(x => new StockItemsView
		{
			StockItemID = x.StockItemID,
			SellingPrice = x.SellingPrice,
			Description = x.Description,
			QuantityOnHand = x.QuantityOnHand
		})
		.ToList();
}

public ReturnSalesView GetSaleRefund(int saleID)
{
	return Sales
		.Where(x => x.SaleID == saleID)
		.Select(x => new ReturnSalesView
		{
			SaleID = x.SaleID,
			EmployeeID = x.EmployeeID,
			TaxAmount = x.TaxAmount,
			SubTotal = x.SubTotal,
			CouponID = x.CouponID ?? 0,
			Reason = "Wrong Item", // added reason since i needed to test reason for business rules
			ReturnSalesDetails = x.SaleDetails
										.Where(c => c.SaleID == saleID)
										.Select(c => new ReturnSalesDetailCartView
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
#endregion

// Sale Models
public class CategoryView 
{
	public int CategoryID {get; set;}
	public string Description {get; set;}
	public int StockItemCount {get; set;}
}

public class StockItemsView
{
	public int StockItemID {get; set;}
	public decimal SellingPrice {get; set;}
	public string Description {get; set;}
	public int QuantityOnHand {get; set;}
}

public class ShoppingCartView
{
	public int StockItemID {get; set;}
	public string Description {get; set;}
	public int Quantity {get; set;}
	public decimal SellingPrice {get; set;}
}

public class SaleView
{
	public int SaleID {get; set;}
	public int EmployeeID {get; set;}
	public string PaymentType {get; set;}
	public decimal TaxAmount {get; set;}
	public decimal SubTotal {get; set;}
	public int CouponID {get; set;}
	public bool Discontinued {get; set;} // added for business rule
	public List<ShoppingCartView> Items {get; set;}
}

// Refund Models
public class ReturnSalesView 
{
	public int SaleID {get; set;}
	public int EmployeeID {get; set;}
	public decimal TaxAmount {get; set;}
	public decimal SubTotal {get; set;}
	public int CouponID {get; set;}
	public string Reason {get; set;} // added for business rule
	public List<ReturnSalesDetailCartView> ReturnSalesDetails {get; set;}
}

public class ReturnSalesDetailCartView
{
	public int StockItemID { get; set; }
	public string Description { get; set; }
	public int OriginalQty { get; set; }
	public decimal SellingPrice { get; set; }
	public int PreviousReturnQty { get; set; }
	public int QtyReturnNow {get; set;}
}