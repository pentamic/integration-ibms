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
        public DbSet<CheckIn> tblCheckIns { get; set; }
        public DbSet<CheckInCar> CheckIn_Cars { get; set; }
        public DbSet<CheckInDetail> CheckIn_Infos { get; set; }
        public DbSet<CheckInPayee> CheckIn_Payes { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Branch> Branchs { get; set; }
        public DbSet<Nationality> Nationalities { get; set; }
        public DbSet<Partner> tblPartners { get; set; }
        public DbSet<PartnerGroup> tblPartnerGroups { get; set; }
        public DbSet<Payee> Payes { get; set; }
        public DbSet<TourGuide> TourGuides { get; set; }
        public DbSet<VisitorType> VisitorTypes { get; set; }
        public DbSet<DataSync> DataSyncs { get; set; }
        public DbSet<Receipt> Receipts { get; set; }
        public DbSet<Customer> tblCustomers { get; set; }
        public DbSet<Saleman> Salemans { get; set; }
        public DbSet<ReceiptSalesStaff> Bill_Sales { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductGroup> ProductGroups { get; set; }
        public DbSet<tmpReceiptDetail> Bill_Products { get; set; }
        public DbSet<ReceiptDetail> ReceiptDetails { get; set; }
        public DbSet<Coffer> Coffers { get; set; }
        public DbSet<Country> tblCountrys { get; set; }
        public DbSet<City> tblCitys { get; set; }
        public DbSet<ReceiptDiscount> ReceiptDiscounts { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<ReceiptPayment> CardPayments { get; set; }
        public DbSet<BalanceStock> BalanceStocks { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<SettingAPI> SettingAPIs { get; set; }
        public DbSet<Inventory> Inventorys { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<ReceiptPayee> ReceiptPayees { get; set; }
        public DbSet<ContactFeePort> ContactFeePorts { get; set; }

    }
}
