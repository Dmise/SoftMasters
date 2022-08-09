using Microsoft.EntityFrameworkCore;
using WebApp.Models;

namespace WebApp.Data
{
    public class SMDbContext : DbContext
    {
       
        public DbSet<Station> Stations { get; set; } 
        public DbSet<Composition> Compositions { get; set; }
        public DbSet<Train> Trains { get; set; } 
        public DbSet<Car> Cars { get; set; }        
        public DbSet<Freight> Freights { get; set; }
        public DbSet<Invoice> Invoices { get; set; }    
        public DbSet<OperationName> OperationNames { get; set; }
        public DbSet<Operation> Operations { get; set; }
        
                
        public SMDbContext(DbContextOptions<SMDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            //modelBuilder.Entity<Train>().Property<string>(t => t.ToStationName).ValueGeneratedOnAdd(); // от этого только проблемы
            //modelBuilder.Entity<Train>().Property<string>(t => t.FromStationName).ValueGeneratedOnAdd();
            //modelBuilder.Entity<Composition>().Property(c => c.TrainId).ValueGeneratedOnAddOrUpdate();

            
            modelBuilder.Entity<Train>().HasKey(t => new { t.TrainId, t.FromStationName, t.ToStationName });
            modelBuilder.Entity<Train>().HasIndex(t => new { t.ToStationName, t.FromStationName }); // это совсем не обязательно
            modelBuilder.Entity<Train>(entity => {
                entity.HasOne(t => t.toStation) // t => t.toStation
                .WithMany(s => s.TrainsTo)
                .HasForeignKey(t => t.ToStationName)
                .OnDelete(DeleteBehavior.Restrict);               
                entity.HasOne(t => t.fromStation)
                .WithMany(s => s.TrainsFrom)
                .HasForeignKey(t => t.FromStationName)
                .OnDelete(DeleteBehavior.Restrict);
            });
            

            
            modelBuilder.Entity<Composition>().HasOne(c => c.Train)
                .WithMany(t => t.Compositions)
                .HasForeignKey(c => c.TrainId)
                .HasPrincipalKey(t => t.TrainId);


            modelBuilder.Entity<Operation>().HasKey(o => new { o.WhenLastOperation, o.CarNumber });
            
           
        }
    }
}

  

