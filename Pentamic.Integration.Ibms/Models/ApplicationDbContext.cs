﻿using System.Data.Entity;
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
        public DbSet<tblCheckIn> tblCheckIns { get; set; }
        public DbSet<CheckIn_Car> CheckIn_Cars { get; set; }
        public DbSet<CheckIn_Info> CheckIn_Infos { get; set; }
        public DbSet<CheckIn_Payee> CheckIn_Payes { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Branch> Branchs { get; set; }
        public DbSet<Nationality> Nationalities { get; set; }
        public DbSet<tblPartner> tblPartners { get; set; }
        public DbSet<tblPartnerGroup> tblPartnerGroups { get; set; }
        public DbSet<Payee> Payes { get; set; }
        public DbSet<TourGuide> TourGuides { get; set; }
        public DbSet<VisitorType> VisitorTypes { get; set; }
        public DbSet<DataSync> DataSyncs { get; set; }
        public DbSet<Receipt> Receipts { get; set; }
        public DbSet<tblCustomer> tblCustomers { get; set; }
        public DbSet<Saleman> Salemans { get; set; }
        public DbSet<Bill_Sale> Bill_Sales { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Bill_Product> Bill_Products { get; set; }
        public DbSet<ReceiptDetail> ReceiptDetails { get; set; }
        public DbSet<Coffer> Coffers { get; set; }
        public DbSet<tblCountry> tblCountrys { get; set; }
        public DbSet<tblCity> tblCitys { get; set; }
        public DbSet<ReceiptDiscount> ReceiptDiscounts { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
    }
}