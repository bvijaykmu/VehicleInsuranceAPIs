

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MetadaService.Core;
using Microsoft.EntityFrameworkCore;

namespace MetadataService.Infrastructure
{
    public class InsuranceContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<VehicleType> VehicleTypes { get; set; }
        public InsuranceContext()
        {
        }
        public InsuranceContext(DbContextOptions<InsuranceContext> options)
            : base(options)
        {
        }
           protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=InsuranceDb;Trusted_Connection=True;");
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Customer Flent API setup
            //shadow property
            modelBuilder.Entity<Customer>()
               .Property<DateTime>("LastUpdated");

            modelBuilder.Entity<Customer>()
               .Property(c => c.PhoneNumber)
               .HasColumnName("Phone")
               .HasMaxLength(10);

            modelBuilder.Entity<Customer>()
            .Property(c => c.FirstName)
            .IsRequired();

            //HasAlternateKey method enables you to create an alternate key by placing a unique constraint 
            modelBuilder.Entity<Customer>()
            .HasAlternateKey(c => c.EmailId);

            //Ignore method is used in the OnModelCreating method to specify that it is to be excluded from mapping 
            modelBuilder.Entity<Customer>()
            .Ignore(c => c.FullName);
            //User A might retrieve a row of customer, followed by User B. The rowversion value for the row will be the same for both users. If User B submits changes, the rowversion value in the table will increment by 1 for that row. If User A subsequently tries to modify the same record, the rowversion value in their WHERE clause combined with the primary key value will no longer match an existing row in the database and EF Core will throw a DbUpdateConcurrencyException
            modelBuilder.Entity<Customer>()
            .Property(c =>c.RowVersion).IsRowVersion();

            //Vehicle
            modelBuilder.Entity<Vehicle>()
               .Property<DateTime>("LastUpdated");
            modelBuilder.Entity<Vehicle>()
               .Property(V => V.Year)
               .HasColumnType("int");


            //Quote
            // The value of the column is generated by the database's GetUtcDate() method whenever the row is created or updated:
            modelBuilder.Entity<Quote>()
             .Property<DateTime>("LastModified")
             .HasComputedColumnSql("GetUtcDate()");
            //HasDefaultValueSql method is used to specify the expression used to generate the default value for a database column mapped to a property
            modelBuilder.Entity<Quote>()
             .Property<DateTime>("DateCreated")
             .HasDefaultValueSql("GetUtcDate()");
            //ValueGeneratedOnAdd method indicates that the value for the selected property is generated by the database whenever a new entity is added to the database
            modelBuilder.Entity<Quote>()
                .Property("DateCreated")
                .ValueGeneratedOnAdd();
            //HasIndex method is used to create a database index on the column mapped to the specified entity property
            modelBuilder.Entity<Quote>()
           .HasIndex(q => new { q.CustomerId, q.StartDate });

            //set foreign constraint name on one to many relation ship
            modelBuilder.Entity<Quote>()
            .HasOne(q => q.Customer)
            .WithMany(c => c.Quotes)
            .HasConstraintName("FK_Customer_Quotes")
            //IsRequired method on the relationship to prevent the relationship being optional
            .IsRequired()
            //OnDelete method is used to specify the action which should take place on a dependent entity in a relationship when the principal is deleted
            .OnDelete(DeleteBehavior.Cascade);



            //VehicleType
            //Seed data
            modelBuilder.Entity<VehicleType>()
            .HasData(new VehicleType[]{
                new VehicleType(){VehicleTypeId=1,VehicleTypeName="SUV"},
                new VehicleType(){VehicleTypeId=2,VehicleTypeName="Sedan"},
                new VehicleType(){VehicleTypeId=3,VehicleTypeName="Truck"},
                new VehicleType(){VehicleTypeId=4,VehicleTypeName="EV"}
            });
            //ValueGeneratedNever provides a way to specify that the value for the selected property should never be generated automtically by the database
            modelBuilder.Entity<VehicleType>()
           .Property(p => p.VehicleTypeId)
           .ValueGeneratedNever();

            //VehicleQuote
            //composite primary key
            modelBuilder.Entity<VehicleQuote>()
                .HasKey(vq => new { vq.VehicleId, vq.QuoteId });
                
            //Many to Many relationship
            modelBuilder.Entity<VehicleQuote>()
                .HasOne(vq => vq.Vehicle)
                .WithMany(v => v.VehicleQuotes)
                .HasForeignKey(vq => vq.VehicleId);

            modelBuilder.Entity<VehicleQuote>()
                .HasOne(vq => vq.Quote)
                .WithMany(q => q.VehicleQuotes)
                .HasForeignKey(vq => vq.QuoteId);



        }
        public  override async  Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            ChangeTracker.DetectChanges();

            foreach (var entry in ChangeTracker.Entries())
            {
                if ((entry.GetType()== typeof(Customer) || entry.GetType()== typeof(Vehicle)) 
                        && (entry.State == EntityState.Added || entry.State == EntityState.Modified))
                {
                    entry.Property("LastUpdated").CurrentValue = DateTime.UtcNow;
                }
            }
            return await base.SaveChangesAsync();
            
        }
    }
}