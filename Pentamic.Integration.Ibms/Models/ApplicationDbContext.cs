using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Pentamic.Integration.Ibms.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
            : base("DefaultConnection")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
        public DbSet<CardMaker> CardMakers { get; set; }
        public DbSet<CardType> CardTypes { get; set; }
        public DbSet<CarNumber> CarNumbers { get; set; }
        public DbSet<CheckIn> CheckIns { get; set; }
        public DbSet<CheckIn_Car> CheckIn_Cars { get; set; }
        public DbSet<CheckIn_Info> CheckIn_Infos { get; set; }
        public DbSet<CheckIn_Payee> CheckIn_Payes { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Nationality> Nationalities { get; set; }
        public DbSet<Partner> Partners { get; set; }
        public DbSet<PartnerGroup> PartnerGroups { get; set; }
        public DbSet<Payee> Payes { get; set; }
        public DbSet<TourGuide> TourGuides { get; set; }
        public DbSet<VisitorType> VisitorTypes { get; set; }
    }
}