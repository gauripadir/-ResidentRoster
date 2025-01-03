namespace SocietyManagementMVC.Models
{
    public class Bill
    {
        public int Id { get; set; }
        public string BillTitle { get; set; }
        public string FlatNumber { get; set; }
        public string BillAmount { get; set; }
        public string? PaidAmount { get; set; }
        public string BillMonth { get; set; }
        public string? PaymentDate { get; set; }
        public string? PaymentMethod { get; set; }
    }
}
