<Query Kind="Statements">
  <Connection>
    <ID>90e2cd68-9273-4285-80ac-606ef523136b</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Server>.</Server>
    <AllowDateOnlyTimeOnly>true</AllowDateOnlyTimeOnly>
    <Database>eTools2023</Database>
    <DriverData>
      <LegacyMFA>false</LegacyMFA>
    </DriverData>
  </Connection>
</Query>

//Get Vendors
Vendors.Select(x=>new{
	VendorID = x.VendorID,
	VendorName = x.VendorName,
	Phone = x.Phone,
	Address = x.Address,
	City = x.City,
	Province = x.Province.Description,
	PostalCode = x.PostalCode
}).Dump();

//Get Employee
Employees.Where(x=>x.EmployeeID == 1).Select(x=>new{
	EmployeeID = x.EmployeeID,
	FullName = x.FirstName + " " + x.LastName
}).Dump();

//Get Vendor Purchase Order
PurchaseOrders.Where(x=>x.PurchaseOrderID == 358).Select(x=>new{
	Vendor = x.Vendor,
	PurchaseOrderID = x.PurchaseOrderID,
	Items = x.PurchaseOrderDetails.Select(x=>new{
		PurchaseOrderDetailID = x.PurchaseOrderDetailID,
		StockItemID = x.StockItemID,
		Description = x.StockItem.Description,
		QOH = x.StockItem.QuantityOnHand,
		ROL = x.StockItem.ReOrderLevel,
		QOO = x.StockItem.QuantityOnOrder,
		QTO = x.StockItem.QuantityOnHand - x.StockItem.ReOrderLevel,
		Price = x.StockItem.PurchasePrice
	}),
	SubTotal = x.SubTotal,
	GST = x.TaxAmount
}).Dump();

//Fetch Inventory By Vendor
StockItems
.Where(x=>x.VendorID == 1)
.Select(x=>new{
	PurchaseOrderDetailID = 0,
	StockItemID = x.StockItemID,
	Description = x.Description,
	QOH = x.QuantityOnHand,
	ROL = x.ReOrderLevel,
	QOO = x.QuantityOnOrder,
	QTO = x.QuantityOnHand - x.ReOrderLevel,
	Price = x.PurchasePrice
}).Dump();

//Fetch Inventory by Vendor AND Purchase Order

StockItems
.Where(x => x.VendorID == 1)
.Select(x => new
{
	PurchaseOrderDetailID = StockItems.Any(item => PurchaseOrderDetails.Where(item=>item.PurchaseOrderID == 358 && x.StockItemID == item.StockItemID).Select(x=>x.StockItemID).Contains(item.StockItemID)) == true ? 1 : 0,
	StockItemID = x.StockItemID,
	Description = x.Description,
	QOH = x.QuantityOnHand,
	ROL = x.ReOrderLevel,
	QOO = x.QuantityOnOrder,
	QTO = x.QuantityOnHand - x.ReOrderLevel,
	Price = x.PurchasePrice
}).Where(x=>x.PurchaseOrderDetailID == 0);
