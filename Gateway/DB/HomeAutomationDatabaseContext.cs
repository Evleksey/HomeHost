using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

#nullable disable

namespace Gateway.DB
{
    public partial class HomeAutomationDatabaseContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public HomeAutomationDatabaseContext(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public HomeAutomationDatabaseContext()
        {
        }

        public HomeAutomationDatabaseContext(DbContextOptions<HomeAutomationDatabaseContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Device> Devices { get; set; }
        public virtual DbSet<Room> Rooms { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_configuration.GetConnectionString("DB"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Device>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Ip)
                    .IsRequired()
                    .HasMaxLength(15)
                    .HasColumnName("ip");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(125)
                    .HasColumnName("name");

                entity.Property(e => e.RoomId).HasColumnName("roomId");

                entity.HasOne(d => d.Room)
                    .WithMany(p => p.Devices)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Devices_Rooms");
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name).HasMaxLength(256);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
