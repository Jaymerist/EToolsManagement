namespace RentalSystem.ViewModels
{
    public class AvailableEquipmentView
    {
        public int RentalEquipmentID { get; set; }
        public string Description { get; set; }
        public string SerialNumber { get; set; }
        public decimal DailyRate { get; set; }
        public string Condition { get; set; }
    }
}
