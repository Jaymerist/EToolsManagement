using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentalSystem.ViewModels
{
    public class RentalDetailView
    {
        public int RentalDetailID { get; set; }
        public int RentalID { get; set; }
        public int RentalEquipmentID { get; set; }
        public decimal RentalDays { get; set; }
        public decimal RentalRate { get; set; }
        public string OutCondition { get; set; }
        public string InCondition { get; set; }
        public decimal DamageRepairCost { get; set; }
        public string Comments { get; set; }
    }
}
