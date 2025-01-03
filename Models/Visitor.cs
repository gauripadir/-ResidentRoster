namespace SocietyManagementMVC.Models
{
    public class Visitor
    {
        public int Id { get; set; }
        public string FlatNumber { get; set; }
        public string VisitorName { get; set; }
        public string VisitorPhone { get; set; }
        public string PersonToVisit { get; set; }
        public string InTime { get; set; }
        public string? OutTime { get; set; }
        public string ReasonToVisit { get; set; }
        public string? OutRemark { get; set; }
        public string Status { get; set; }
        public string CreatedAtDate { get; set; }
    }
}
