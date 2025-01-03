namespace SocietyManagementMVC.Models
{
    public class Complaint
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string FlatNumber { get; set; }
        public string ComplaintDescription { get; set; }
        public string? Comment { get; set; }
        public string Status { get; set; }
        public string CreatedAtDate { get; set; }
    }
}
