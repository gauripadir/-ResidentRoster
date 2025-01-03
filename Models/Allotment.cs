namespace SocietyManagementMVC.Models
{
    public class Allotment
    {
        public int Id { get; set; }
        public string AllotedTo { get; set; }
        public string FlatNumber { get; set; }
        public string Type { get; set; }
        public string MoveInDate { get; set; }
        public string? MoveOutDate { get; set; }
    }
}
