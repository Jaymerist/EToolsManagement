<Query Kind="Program">
  <Connection>
    <ID>0abe9ff6-5746-4300-af62-232917cbbd20</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Driver Assembly="(internal)" PublicKeyToken="no-strong-name">LINQPad.Drivers.EFCore.DynamicDriver</Driver>
    <Server>DESKTOP-RU1SHSD</Server>
    <Database>eTools2023</Database>
    <DisplayName>eTools2023-Entity</DisplayName>
    <DriverData>
      <EncryptSqlTraffic>True</EncryptSqlTraffic>
      <PreserveNumeric1>True</PreserveNumeric1>
      <EFProvider>Microsoft.EntityFrameworkCore.SqlServer</EFProvider>
    </DriverData>
  </Connection>
</Query>

void Main()
{
	
}
#region Methods
public List<OutstandingOrderView> PurchaseOrders_FetchOutstandingOrders()
{
	return PurchaseOrders
				.Where(po => po.Closed == false && po.OrderDate != null)
				.Select(po => new OutstandingOrderView
				{
					PurchaseOrderID = po.PurchaseOrderID,
					OrderDate = po.OrderDate,
					VendorName = po.Vendor.VendorName,
					VendorPhone = po.Vendor.Phone
				})
				.ToList();
}
public ReceivingView PurchaseOrders_FetchOrderDetails(int orderid)
{
	VendorView vendor = Vendors
							.Where(v => v.VendorID == PurchaseOrders.Where(po => po.PurchaseOrderID == orderid).Select(po => po.VendorID).FirstOrDefault())
							.Select(v => new VendorView
							{
								VendorName = v.VendorName,
								Address = v.Address,
								City = v.City,
								Province = v.Province.Description,
								PostalCode = v.PostalCode,
								VendorPhone = v.Phone
							})
							.FirstOrDefault();
	ReceivingView purchaseOrder = PurchaseOrders
							.Where(po => po.PurchaseOrderID == orderid)
							.Select(po => new ReceivingView
							{
								PurchaseOrderID = po.PurchaseOrderID,
								OrderDate = po.OrderDate,
								Vendor = vendor,
								CanBeClosed = po.Closed,
								ItemDetails = po.PurchaseOrderDetails.Select(pod => new ItemDetailView
								{
									PurchaseOrderDetailId = pod.PurchaseOrderDetailID,
									StockItemId = pod.StockItemID,
									StockItemDescription = pod.StockItem.Description,
									QtyOnOrder = pod.StockItem.QuantityOnOrder,
									QtyOutstanding = pod.StockItem.
									
								}
								)
								
							});
							
	return purchaseOrder;
}
#endregion
#region Views

public class OutstandingOrderView
{
	public int PurchaseOrderID { get; set; }
	public DateTime? OrderDate { get; set; }
	public string VendorName { get; set; }
	public string VendorPhone { get; set; }
}
public class ReceivingView
{
	public int PurchaseOrderID { get; set; }
	public DateTime? OrderDate { get; set; }
	public VendorView Vendor { get; set; }
	public bool CanBeClosed { get; set; }
	public List<ItemDetailView> ItemDetails { get; set; }
}
public class VendorView
{
	public string VendorName { get; set; }
	public string Address { get; set; }
	public string City { get; set; }
	public string Province { get; set; }
	public string PostalCode { get; set; }
	public string VendorPhone { get; set; }
}
public class ItemDetailView
{
	public int PurchaseOrderDetailId { get; set; }
	public int StockItemId { get; set; }
	public string StockItemDescription { get; set; }
	public int QtyOnOrder { get; set; }
	public int QtyOutstanding { get; set; }
	public int QtyReturn { get; set; }
}
#endregion