using MicroService.Models;
using Microsoft.EntityFrameworkCore;

namespace MicroService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Student> Students { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Student>().HasKey(i => i.Id);
            modelBuilder.Entity<Student>().Property(i => i.LastName).HasMaxLength(256);
            modelBuilder.Entity<Student>().Property(i => i.FirstName).HasMaxLength(256);
            modelBuilder.Entity<Student>().Property(i => i.MiddleName).HasMaxLength(256);
            modelBuilder.Entity<Student>().Property(i => i.Speciality).HasMaxLength(256);
            modelBuilder.Entity<Student>().Property(i => i.IsExpelled).HasMaxLength(256);
            modelBuilder.Entity<Student>().Property(i => i.IsExpelled).HasDefaultValue(false);
        }
    }
}
