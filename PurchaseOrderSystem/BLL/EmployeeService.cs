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
    public class EmployeeService
    {
        private readonly eTools2023Context? _eTools2023Context;
        internal EmployeeService(eTools2023Context eToolsContext)
        {
            _eTools2023Context = eToolsContext;
        }
        public EmployeeView GetEmployee(int employeeID)
        {
            return _eTools2023Context.Employees.Where(x => x.EmployeeID == employeeID).Select(x => new EmployeeView
            {
                EmployeeID = x.EmployeeID,
                FullName = x.FirstName + " " + x.LastName
            }).FirstOrDefault();
        }
    }
}
