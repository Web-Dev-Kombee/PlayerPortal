using PlayerPortal.Data.Infrastructure.Tables;
using Microsoft.EntityFrameworkCore;

namespace PlayerPortal.Data.Infrastructure
{
    public class EMDboDBContext : DbContext
    {
        public EMDboDBContext(DbContextOptions<EMDboDBContext> options)
                : base(options)
        {
        }
        public DbSet<PlayerTable> Players { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<PlayerTable>(entity =>
            {
                entity.ToTable("Players");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.ShirtNo)
                      .IsRequired();

                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.Appearance)
                      .IsRequired();

                entity.Property(e => e.Goals)
                      .IsRequired();
            });
        }
    }
}
