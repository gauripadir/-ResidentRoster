using Microsoft.EntityFrameworkCore;
using SocietyManagementMVC.Models;

namespace SocietyManagementMVC.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Flat> Flats { get; set; }
        public DbSet<Allotment> Allotments { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<Visitor> Visitors { get; set; }
        public DbSet<Notification> Notifications { get; set; }
    }
}
