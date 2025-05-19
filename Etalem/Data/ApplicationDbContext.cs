using Etalem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Etalem.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseCategory> CoursesCategory { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<CourseCategory>()
                .HasKey(cc => new { cc.CourseId, cc.CategoryId });

            builder.Entity<CourseCategory>()
                .HasOne(cc => cc.Course)
                .WithMany(c => c.CourseCategories)
                .HasForeignKey(cc => cc.CourseId);

            //builder.Entity<CourseCategory>()
            //    .HasOne(cc => cc.Category)
            //    .WithMany(c => c.CourseCategories)
            //    .HasForeignKey(cc => cc.CategoryId);

            //// العلاقة بين Course و ApplicationUser
            //builder.Entity<Course>()
            //    .HasOne(c => c.Instructor)
            //    .WithMany(u => u.Courses)
            //    .HasForeignKey(c => c.InstructorId);
        }
    }
}
