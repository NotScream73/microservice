using Microsoft.EntityFrameworkCore;
using Account.Models;

namespace Account.Data
{
    public class DataContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DataContext() { }

        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("users").HasKey(i => i.Id);
            modelBuilder.Entity<User>().Property(i => i.Id).UseIdentityColumn();
            modelBuilder.Entity<User>().Property(i => i.FirstName).HasColumnType("varchar(256)");
            modelBuilder.Entity<User>().Property(i => i.LastName).HasColumnType("varchar(256)");
            modelBuilder.Entity<User>().Property(i => i.UserName).HasColumnType("varchar(256)");
            modelBuilder.Entity<User>().HasIndex(i => i.UserName).IsUnique();

            modelBuilder.Entity<Role>().ToTable("roles").HasKey(i => i.Id);
            modelBuilder.Entity<Role>().Property(i => i.Id).UseIdentityColumn();
            modelBuilder.Entity<Role>().Property(i => i.Name).HasColumnType("varchar(256)");
            modelBuilder.Entity<Role>().HasIndex(i => i.Name).IsUnique();

            modelBuilder.Entity<UserRole>().ToTable("user_roles").HasKey(i => new { i.UserId, i.RoleId });
            modelBuilder.Entity<UserRole>().Property(i => i.UserId).ValueGeneratedNever();
            modelBuilder.Entity<UserRole>().Property(i => i.RoleId).ValueGeneratedNever();
            modelBuilder.Entity<UserRole>().HasOne(i => i.Role).WithMany().HasForeignKey(i => i.RoleId);
            modelBuilder.Entity<UserRole>().HasOne<User>().WithMany(i => i.UserRoles).HasForeignKey(i => i.RoleId);

            modelBuilder.Entity<RefreshToken>().ToTable("refresh_tokens").HasKey(i => i.UserId);
            modelBuilder.Entity<RefreshToken>().Property(i => i.Token).HasColumnType("varchar(256)");
            modelBuilder.Entity<RefreshToken>().HasOne(i => i.User).WithOne().HasForeignKey<RefreshToken>(s => s.UserId);
        }
    }
}
