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
            builder.Entity<StudentCourse>().HasKey(sc => new { sc.StudentId, sc.CourseId });

            builder.Entity<StudentCourse>()
                .HasOne<Student>(sc => sc.student)
                .WithMany(s => s.StudentCourses)
                .HasForeignKey(sc => sc.StudentId);


            builder.Entity<StudentCourse>()
                .HasOne<Course>(sc => sc.course)
                .WithMany(s => s.StudentCourses)
                .HasForeignKey(sc => sc.CourseId);

        }
        public DbSet<ApplicationUser> appUsers { get; set; }
        public DbSet<Address> addresses { get; set; }
        public DbSet<Token> tokens { get; set; }
        public DbSet<Activity> activities { get; set; }
        public DbSet<CountryModel> countries { get; set; }
        public DbSet<Student> students { get; set; }
        public DbSet<Course> courses { get; set; }
        public DbSet<StudentCourse> studentCourses { get; set; }
    }
}
