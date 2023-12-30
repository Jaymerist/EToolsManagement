<Query Kind="Program">
  <Connection>
    <ID>9f563eab-5eb5-4add-b387-5be4e456b0da</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Driver Assembly="(internal)" PublicKeyToken="no-strong-name">LINQPad.Drivers.EFCore.DynamicDriver</Driver>
    <Server>.</Server>
    <Database>eTools2023</Database>
    <DisplayName>eTools2023 Entity</DisplayName>
    <DriverData>
      <EncryptSqlTraffic>True</EncryptSqlTraffic>
      <PreserveNumeric1>True</PreserveNumeric1>
      <EFProvider>Microsoft.EntityFrameworkCore.SqlServer</EFProvider>
    </DriverData>
  </Connection>
</Query>

void Main()
{
	try
	{
		
		#region Driver
		//----------------Suggest a purchase order (if QTO is positive, the item will be added to the suggested order), 
		//call list of vendors
		
		VendorView testVendor = GetVendors().Where(x=>x.VendorID == 1).FirstOrDefault();
		testVendor.Dump();
		var newOrder = SuggestPurchaseOrder(testVendor);
		newOrder.Dump(); //Will return null if the vendor has an open order already.


		//----------------Save the suggested order
		//The employee would add the remaining purchase order information if they would like to save
		PurchaseOrderEditView addNewOrder = new PurchaseOrderEditView();
		addNewOrder.VendorID = testVendor.VendorID;
		addNewOrder.EmployeeID = GetEmployee(1).EmployeeID;
		addNewOrder.SubTotal = newOrder.SubTotal;
		addNewOrder.GST = newOrder.GST;
		addNewOrder.ItemDetails = new List<ItemDetailView>();
		
		foreach(ItemView item in newOrder.Items){
			ItemDetailView newItem = new ItemDetailView();
			newItem.Price = item.Price;
			newItem.QTO = item.QTO;
			newItem.StockItemID = item.StockItemID;
			
			addNewOrder.ItemDetails.Add(newItem);
		}

		UpdatePurchaseOrder(addNewOrder, false);
		//Show the purchase orders
		PurchaseOrders.Select(x=>x).Dump();

		//-----------------Add item to existing order
		ItemView testItem = FetchInventoryBy(3).Where(x => x.StockItemID == 5594).FirstOrDefault();
		testItem.QTO = 1000;

		AddItemToPurchaseOrderLine(testItem, 359);
		
		//show in database
		PurchaseOrderDetails.Where(x=>x.PurchaseOrderID == 359).Dump();
		
		//------------------Delete item from purchase order
		RemovePurchaseOrderLine(testItem, 359);
		PurchaseOrderDetails.Where(x=>x.PurchaseOrderID == 359).Dump();

		//------------------Place Order using UpdatePurchaseOrder
		PurchaseOrderEditView updateOrder = new PurchaseOrderEditView();
		updateOrder.PurchaseOrderID = 360;
		updateOrder.EmployeeID = PurchaseOrders.Where(x=>x.EmployeeID == 1).Select(x=>x.EmployeeID).FirstOrDefault();
		updateOrder.VendorID =  GetVendorPurchaseOrder(360).Vendor.VendorID;
		updateOrder.SubTotal = GetVendorPurchaseOrder(360).SubTotal;
		updateOrder.GST = GetVendorPurchaseOrder(360).GST;

		updateOrder.ItemDetails = new List<ItemDetailView>();

		foreach (StockItems item in PurchaseOrderDetails.Where(x=>x.PurchaseOrderID == 360).Select(x=>x.StockItem))
		{
			ItemDetailView newItem = new ItemDetailView();
			newItem.Price = item.PurchasePrice;
			newItem.QTO = item.QuantityOnOrder;
			newItem.StockItemID = item.StockItemID;

			updateOrder.ItemDetails.Add(newItem);
		}
		
		UpdatePurchaseOrder(updateOrder, true);
		//Display PurchaseOrder 360
		PurchaseOrders.Where(x=>x.PurchaseOrderID == 360).Dump();
		
		//----------------Delete purchase order 
		
		//Before delete:
		PurchaseOrders.Where(x=>x.RemoveFromViewFlag == false).Dump();
		DeletePurchaseOrder(348);
		
		//Show purchase orders that do not have RemoveFromViewFlag turned on
		PurchaseOrders.Where(x=>x.RemoveFromViewFlag == false).Dump();

		//----------------Query Methods
		
		GetVendors().Dump();
		
		GetEmployee(1).Dump();
		
		var testFetchOrder = GetVendorPurchaseOrder(352);
		
		testFetchOrder.Dump();
		
		FetchInventoryBy(1).Dump();
		
		FetchInventoryBy(testFetchOrder, testFetchOrder.Vendor.VendorID).Dump();

		#endregion
	}
	#region catch exceptions
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
	#endregion
}


// You can define other methods, fields, classes and namespaces here

#region Methods
private Exception GetInnerException(Exception ex)
{
	while (ex.InnerException != null)
		ex = ex.InnerException;
	return ex;
}





#region Query Methods
public List<VendorView> GetVendors()
{
	List<VendorView> vendorViews = Vendors.Select(x => new VendorView
	{
		VendorID = x.VendorID,
		VendorName = x.VendorName,
		Phone = x.Phone,
		Address = x.Address,
		City = x.City,
		Province = x.Province.Description,
		PostalCode = x.PostalCode
	}).ToList();
	
	return vendorViews;


}
public EmployeeView GetEmployee(int employeeID)
{
	EmployeeView employee = Employees.Where(x => x.EmployeeID == 1).Select(x => new EmployeeView
	{
		EmployeeID = x.EmployeeID,
		FullName = x.FirstName + " " + x.LastName
	}).FirstOrDefault();
	
	return employee;
}
public PurchaseOrderView GetVendorPurchaseOrder(int purchaseOrderID)
{
	PurchaseOrderView purchaseOrder = PurchaseOrders
		.Where(x => x.PurchaseOrderID == purchaseOrderID)
		.Select(x => new PurchaseOrderView
		{
			Vendor = new VendorView
			{
				VendorID = x.Vendor.VendorID,
				VendorName = x.Vendor.VendorName,
				Phone = x.Vendor.Phone,
				Address = x.Vendor.Address,
				City = x.Vendor.City,
				Province = x.Vendor.Province.Description,
				PostalCode = x.Vendor.PostalCode
			},
			PurchaseOrderID = x.PurchaseOrderID,
			Items = x.PurchaseOrderDetails.Select(pd => new ItemView
			{
				PurchaseOrderDetailID = pd.PurchaseOrderDetailID,
				StockItemID = pd.StockItem.StockItemID,
				Description = pd.StockItem.Description,
				QOH = pd.StockItem.QuantityOnHand,
				ROL = pd.StockItem.ReOrderLevel,
				QOO = pd.StockItem.QuantityOnOrder,
				QTO = pd.StockItem.QuantityOnHand - pd.StockItem.ReOrderLevel,
				Price = pd.StockItem.PurchasePrice
			}).ToList(),
			SubTotal = x.SubTotal,
			GST = x.TaxAmount
		})
		.FirstOrDefault();

	return purchaseOrder;
}
public List<ItemView> FetchInventoryBy(int vendorID)
{
	List<ItemView> inventory = StockItems
		.Where(x => x.VendorID == vendorID)
		.Select(x => new ItemView
		{
			PurchaseOrderDetailID = 0,
			StockItemID = x.StockItemID,
			Description = x.Description,
			QOH = x.QuantityOnHand,
			ROL = x.ReOrderLevel,
			QOO = x.QuantityOnOrder,
			QTO = x.QuantityOnHand - x.ReOrderLevel,
			Price = x.PurchasePrice
		})
		.ToList();

	return inventory;
}

public List<ItemView> FetchInventoryBy(PurchaseOrderView purchaseOrderInfo, int vendorID)
{
	List<ItemView> inventory = StockItems
	.Where(x => x.VendorID == vendorID)
	.Select(x => new ItemView
	{
		PurchaseOrderDetailID = StockItems.Any(item => PurchaseOrderDetails.Where(item => item.PurchaseOrderID == purchaseOrderInfo.PurchaseOrderID && x.StockItemID == item.StockItemID).Select(x => x.StockItemID).Contains(item.StockItemID)) == true ? 1 : 0,
		StockItemID = x.StockItemID,
		Description = x.Description,
		QOH = x.QuantityOnHand,
		ROL = x.ReOrderLevel,
		QOO = x.QuantityOnOrder,
		QTO = x.QuantityOnHand - x.ReOrderLevel,
		Price = x.PurchasePrice
	}).Where(x => x.PurchaseOrderDetailID == 0).ToList();

	return inventory;
}


#endregion

#region CRUD Opperation Methods
//Populate a PurchaseOrder when a vendor with no active order is selected
public PurchaseOrderView SuggestPurchaseOrder(VendorView vendor)
{

	//we need a container to hold x number of Exception messages
	List<Exception> errorList = new List<Exception>();

	//check if Vendor exists
	if (!Vendors.Any(x => x.VendorID == vendor.VendorID))
	{
		errorList.Add(new Exception("Vendor does not exist"));
	}

	if (PurchaseOrders.Where(x => x.VendorID == vendor.VendorID && x.OrderDate == null && x.RemoveFromViewFlag == false).Count() == 0)
	{
		//There are no active orders
		//Create a suggested PO
		PurchaseOrderView newOrder = new PurchaseOrderView();
			
		newOrder.Vendor = vendor;

		//Suggest items
		//get inventory of vendor
		var Inventory = FetchInventoryBy(vendor.VendorID);
		newOrder.Items = new List<ItemView>();
		foreach (ItemView item in Inventory)
		{
			//add if ReorderLevel is higher than QuantityOnHand
			if(item.QTO > 0){
				newOrder.Items.Add(item);
			}

		}
		
		newOrder.SubTotal = newOrder.Items.Sum(x=>x.Price * x.QTO);
		newOrder.GST = newOrder.SubTotal * 0.05m;

		if (errorList.Count > 0)
		{
			// Clear changes to maintain data integrity.
			ChangeTracker.Clear();
			string errorMsg = "Please check error message(s)";
			throw new AggregateException(errorMsg, errorList);
		}
		else
		{
			return newOrder;
		}

	}else{

		if (errorList.Count > 0)
		{
			// Clear changes to maintain data integrity.
			ChangeTracker.Clear();
			string errorMsg = "Please check error message(s)";
			throw new AggregateException(errorMsg, errorList);
		}
		else
		{
			return null;
		}
		//there is no need to suggest an order. Open the existing order.
		
	}
}

//the button using this method will send information on the purchase order and the item needing to be added.
public void AddItemToPurchaseOrderLine(ItemView item, int poID)
{

	//we need a container to hold x number of Exception messages
	List<Exception> errorList = new List<Exception>();

	//Rules:
	//The user selects a stock item from the table and does not input anything about the item. It should be assumed that the item
	//exists in the database and has valid data if it's being shown to the user in the first place.
	
	if (!PurchaseOrders.Any(x => x.PurchaseOrderID == poID && x.RemoveFromViewFlag == false))
	{
		errorList.Add(new Exception("Purchase order does not exist in database."));
	}

	//item must be sold by the vendor of the Purchase Order
	//retrieve vendor ID
	int vendorID = PurchaseOrders.Where(x=>x.PurchaseOrderID == poID).Select(x=>x.VendorID).FirstOrDefault();
	
	if(!StockItems.Where(x=>x.VendorID == vendorID).Any(x=>x.StockItemID == item.StockItemID)){
		errorList.Add(new Exception("Item does not exist with chosen vendor."));
	}

	//Check if item does not already exist in purchase order items list. Set view flag to false if true.
	var itemLine = new PurchaseOrderDetails();

	if (PurchaseOrderDetails.Where(x => x.PurchaseOrderID == poID && x.RemoveFromViewFlag == true).Any(existingItem => existingItem.StockItemID == item.StockItemID))
	{
		// Item already exists in the purchase order but view view flag is true.
		itemLine = PurchaseOrderDetails.Where(x => x.PurchaseOrderID == poID).FirstOrDefault();
		itemLine.RemoveFromViewFlag = false;

		//update
		PurchaseOrderDetails.Update(itemLine);
	}
	else if (PurchaseOrderDetails.Where(x => x.PurchaseOrderID == poID).Any(existingItem => existingItem.StockItemID == item.StockItemID))
	{
		//item exists and is still viewable. Do not add again.
		errorList.Add(new Exception("Item already exists in the purchase order."));
	}
	else
	{
		//continue creating a new item line
		itemLine.PurchaseOrderID = poID;
		itemLine.StockItemID = item.StockItemID;
		itemLine.PurchasePrice = item.Price;
		itemLine.Quantity = item.QTO;

		//add
		PurchaseOrderDetails.Add(itemLine);
	}

if (errorList.Count > 0)
	{
		// Clear changes to maintain data integrity.
		ChangeTracker.Clear();
		string errorMsg = "Please check error message(s)";
		throw new AggregateException(errorMsg, errorList);
	}
	else
	{
		// Persist changes to the database.
		SaveChanges();
	}
}

//Delete item from purchase order items list
public void RemovePurchaseOrderLine(ItemView item, int poID)
{
	//we need a container to hold x number of Exception messages
	List<Exception> errorList = new List<Exception>();

	//Rules:
	//The user selects a stock item from the table and does not input anything about the item. It should be assumed that the item
	//exists in the database and has valid data if it's being shown to the user in the first place.

	//Check if item does not already exist in purchase order items list
	var itemLine = PurchaseOrderDetails.Where(x=>x.PurchaseOrderID == poID && x.StockItemID == item.StockItemID).FirstOrDefault();

	if (!PurchaseOrderDetails.Where(x=>x.PurchaseOrderID == poID).Any(existingItem => existingItem.StockItemID == item.StockItemID))
	{
		// Item does not exist in the purchase order, throw an exception or handle accordingly
		errorList.Add(new Exception("Unable to remove item from purchase order."));
	}

	//set remove from view flag to true to show the user that the purchase item is gone (not deleted permanently from the DB)
	itemLine.RemoveFromViewFlag = true;

	if (errorList.Count > 0)
	{
		// Clear changes to maintain data integrity.
		ChangeTracker.Clear();
		string errorMsg = "Please check error message(s)";
		throw new AggregateException(errorMsg, errorList);
	}
	else
	{
		// Persist changes to the database.
		SaveChanges();
	}
}

public void PlacePurchaseOrder(PurchaseOrderEditView order)
{
	//we need a container to hold x number of Exception messages
	List<Exception> errorList = new List<Exception>();

	//If placeOrder = true, place the order.
		//make sure the purchase order has not already been place
		//purchase orders cannot be placed if there are no items
		if (order.OrderDate == null)
		{
			if (PurchaseOrderDetails.Where(x => x.PurchaseOrderID == order.PurchaseOrderID).Count() != 0)
			{
				//will set the OrderDate for the current order
				order.OrderDate = DateTime.Now;
				PurchaseOrders.Update(newOrder);


			}
			else
			{
				errorList.Add(new Exception("The order cannot be placed without any items on it."));
			}
		}
		else
		{
			errorList.Add(new Exception("The order has already been placed. It cannot be placed again."));
		}

}

//This can be used for the Update or Place button
public PurchaseOrderView UpdatePurchaseOrder(PurchaseOrderEditView order, bool placeOrder)
{
	//we need a container to hold x number of Exception messages
	List<Exception> errorList = new List<Exception>();
	PurchaseOrders newOrder = new PurchaseOrders();
	//Rules:
	//The order ID must exist

	//Suggested purchase orders should be saved, not updated.
	if(order.PurchaseOrderID == 0){
		newOrder.VendorID = order.VendorID;
		newOrder.EmployeeID = order.EmployeeID;
		newOrder.TaxAmount = order.GST;
		newOrder.SubTotal = order.SubTotal;
		newOrder.Employee = Employees.Where(x=>x.EmployeeID == newOrder.EmployeeID).FirstOrDefault();
		newOrder.Vendor = Vendors.Where(x=>x.VendorID == newOrder.VendorID).FirstOrDefault();

		//Save the item details
		foreach (ItemDetailView itemLine in order.ItemDetails)
		{
			PurchaseOrderDetails poDetail = new PurchaseOrderDetails();
			poDetail.StockItemID = itemLine.StockItemID;
			poDetail.PurchasePrice = itemLine.Price;
			poDetail.Quantity = poDetail.Quantity;

			//update the QuantityOnOrder for each stock item on the purchase order if the order is being placed
			if (placeOrder == true)
			{
				StockItems item = StockItems.Where(x => x.StockItemID == poDetail.StockItemID).FirstOrDefault();
				item.QuantityOnOrder = poDetail.Quantity;

				StockItems.Update(item);
			}
			newOrder.PurchaseOrderDetails.Add(poDetail);
			
		}
		PurchaseOrders.Add(newOrder);
	}
	else
	{
		//Purchase order ID needs to exist and not be deleted
		if (!PurchaseOrders.Any(x => x.PurchaseOrderID == order.PurchaseOrderID && x.RemoveFromViewFlag == false))
		{
			errorList.Add(new Exception("Purchase order does not exist in database."));
		}
		newOrder = PurchaseOrders.Where(x => x.PurchaseOrderID == order.PurchaseOrderID).FirstOrDefault();
		newOrder.VendorID = order.VendorID;
		newOrder.EmployeeID = order.EmployeeID;
		newOrder.TaxAmount = order.GST;
		newOrder.SubTotal = order.SubTotal;

		//remove purchase order items that exist in the db but are not on the new list of items
		List<PurchaseOrderDetails> existingOrderList = PurchaseOrderDetails.Where(x=>x.PurchaseOrderID == order.PurchaseOrderID && x.RemoveFromViewFlag == false).ToList();
		
		foreach(var item in existingOrderList){
			if(!order.ItemDetails.Any(x=>x.StockItemID == item.StockItemID)){
				var deleteItem = new ItemView();
				deleteItem.StockItemID = item.StockItemID;
				RemovePurchaseOrderLine(deleteItem, order.PurchaseOrderID); 
			}
		}

		//Save the item details
		foreach (ItemDetailView itemLine in order.ItemDetails)
		{
			PurchaseOrderDetails poDetail = new PurchaseOrderDetails();
			poDetail.PurchaseOrderID = order.PurchaseOrderID;
			poDetail.StockItemID = itemLine.StockItemID;
			poDetail.PurchasePrice = itemLine.Price;
			poDetail.Quantity = poDetail.Quantity;

			//update the QuantityOnOrder for each stock item on the purchase order
			StockItems item = StockItems.Where(x => x.StockItemID == poDetail.StockItemID).FirstOrDefault();
			item.QuantityOnOrder = poDetail.Quantity;

			StockItems.Update(item);

			//check if line already exists to add or update it
			if(PurchaseOrderDetails.Where(x=>x.PurchaseOrderID == order.PurchaseOrderID && x.StockItemID == poDetail.StockItemID).Count() != 0){
				PurchaseOrderDetails.Update(poDetail);
			}else{
				PurchaseOrderDetails.Add(poDetail);
			}
		}
		
		PurchaseOrders.Update(newOrder);
	}

	if (errorList.Count > 0)
	{
		// Clear changes to maintain data integrity.
		ChangeTracker.Clear();
		string errorMsg = "Please check error message(s)";
		throw new AggregateException(errorMsg, errorList);
	}
	else
	{
		// Persist changes to the database.
		SaveChanges();
	}
	
	
	//save to PurchaseOrderView to redisplay 
	var newPOView = GetVendorPurchaseOrder(newOrder.PurchaseOrderID);
	return newPOView;

}

public bool DeletePurchaseOrder(int purchaseOrderID)
{
	//we need a container to hold x number of Exception messages
	List<Exception> errorList = new List<Exception>();

	//Rules:
	//The order ID must exist and not already be deleted (RemoveFromViewFlag on)
	if (!PurchaseOrders.Any(x => x.PurchaseOrderID == purchaseOrderID && x.RemoveFromViewFlag == false))
	{
		errorList.Add(new Exception("Purchase order does not exist in database."));
	}
	//Placed orders cannot be deleted
	PurchaseOrders order = PurchaseOrders.Where(x=>x.PurchaseOrderID == purchaseOrderID).FirstOrDefault();
	if (order.OrderDate != null)
	{
		// Order has already been placed, throw an error
		errorList.Add(new Exception("Placed orders cannot be deleted."));
	}

	//set remove from view flag to true 
	order.RemoveFromViewFlag = true;
	
	PurchaseOrders.Update(order);

	if (errorList.Count > 0)
	{
		// Clear changes to maintain data integrity.
		ChangeTracker.Clear();
		string errorMsg = "Please check error message(s)";
		throw new AggregateException(errorMsg, errorList);
	}
	else
	{
		// Persist changes to the database.
		SaveChanges();
		return true; //use this to check if the record was deleted and send a confirmation message to the user.
	}
}
#endregion


#endregion

#region View Models
public class PurchaseOrderView
{
	public VendorView Vendor { get; set; }
	public int PurchaseOrderID { get; set; }
	public List<ItemView> Items { get; set; }
	public decimal SubTotal { get; set; }
	public decimal GST { get; set; }
}


public class VendorView
{
	public int VendorID { get; set; }
	public string VendorName { get; set; }
	public string Phone { get; set; }
	public string Address { get; set; }
	public string City { get; set; }
	public string Province { get; set; }
	public string PostalCode { get; set; }
}

public class EmployeeView
{
	public int EmployeeID { get; set; }
	public string FullName { get; set; }
}

public class ItemView
{
	public int PurchaseOrderDetailID { get; set; }
	public int StockItemID { get; set; }
	public string Description { get; set; }
	public int QOH { get; set; } // Quanity on Hand
	public int ROL { get; set; } // Reoreder Level
	public int QOO { get; set; } // Quantity on Order
	public int QTO { get; set; } // Quantity to Order
	public decimal Price { get; set; }
}

public class PurchaseOrderEditView
{
	public int PurchaseOrderID { get; set; }
	public int VendorID { get; set; }
	public int EmployeeID { get; set; }
	public List<ItemDetailView> ItemDetails { get; set; }
	public decimal SubTotal { get; set; }
	public decimal GST { get; set; }
}

public class ItemDetailView
{
	public int StockItemID { get; set; }
	public int QTO { get; set; }
	public decimal Price { get; set; }
}




#endregion