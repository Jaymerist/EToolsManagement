#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PurchaseOrderSystem.DAL;
using PurchaseOrderSystem;
using PurchaseOrderSystem.ViewModels;


namespace PurchaseOrderSystem.BLL
{
    public class VendorService
    {
        private readonly eTools2023Context? _eTools2023Context;
        internal VendorService(eTools2023Context eToolsContext)
        {
            _eTools2023Context = eToolsContext;
        }
        public List<VendorView> GetVendors()
        {
            return _eTools2023Context.Vendors.Select(x => new VendorView
            {
                VendorID = x.VendorID,
                VendorName = x.VendorName,
                Phone = x.Phone,
                Address = x.Address,
                City = x.City,
                Province = x.ProvinceID,
                PostalCode = x.PostalCode
            }).ToList();
        }
    }
}
