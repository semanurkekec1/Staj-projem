using Microsoft.AspNetCore.Identity;

namespace Classroom.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public List<Class_User>? Class_Users { get; set; }
        public List<Homework_User>? Homework_Users { get; set; }
        public List<Homework>? Homeworks { get; set; }
        public List<Announcements>? Announcementss { get; set; }
        public List<ClassRoom>? ClassRooms { get; set; }
    }
}
