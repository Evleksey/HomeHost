using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace LoggingService.Models
{
    public partial class HomeAutomationDatabaseContext : DbContext
    {
        public HomeAutomationDatabaseContext()
        {
        }

        public HomeAutomationDatabaseContext(DbContextOptions<HomeAutomationDatabaseContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Log> Logs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=192.168.1.83;Database=HomeAutomationDatabase;User ID=server;Password=****;Trusted_Connection=False;Encrypt=True;Connection Timeout=2400;MultipleActiveResultSets=True;trustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Log>(entity =>
            {
                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Datetime)
                    .HasColumnType("datetime")
                    .HasColumnName("datetime");

                entity.Property(e => e.EventType)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("eventType");

                entity.Property(e => e.Message)
                    .HasMaxLength(256)
                    .HasColumnName("message");

                entity.Property(e => e.ObjId).HasColumnName("objId");
            });            

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
