namespace SocietyManagementMVC.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string NotificationName { get; set; }
        public string NotificationFor { get; set; }
        public string FlatNumber { get; set; }
        public string NotificationMessage { get; set; }
        public string Status { get; set; }
        public string Seen { get; set; }
    }
}
