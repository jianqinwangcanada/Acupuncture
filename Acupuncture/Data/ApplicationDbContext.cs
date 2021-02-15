using System;
using Acupuncture.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Acupuncture.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):
            base(options)
        { }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<IdentityRole>().HasData(
                new { Id = "1", Name = "Administrator", NormalizedName = "ADMINISTRATOR", RoleName = "Administrator", Handle = "administrator", RoleIcon = "/uploads/roles/icons/default/role.png", IsActive = true },
                new { Id = "2", Name = "Customer", NormalizedName = "CUSTOMER", RoleName = "customer", Handle = "customer", RoleIcon = "/uploads/roles/icons/default/role.png", IsActive = true }
            );
              
        }
        public DbSet<ApplicationUser> appUsers { get; set; }
        public DbSet<Address> addresses { get; set; }
        public DbSet<Token> tokens { get; set; }
    }
}
