using Classroom.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Classroom.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<ClassRoom> ClassRoom { get; set; }
        public DbSet<Class_User> ClassUser { get; set; }
        public DbSet<Homework> Homework { get; set; }
        public DbSet<Homework_User> HomeworkUser { get; set; }
        public DbSet<Announcements> Announcements { get; set; }
        public DbSet<Comment> Comment { get; set; }
    }
}
