using Classroom.Models;

namespace Classroom.Models
{
    public class ClassRoom
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? UnicCode {  get; set; }
        public string? Color { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public List<Class_User>? ClassUser { get; set; }
        public List<Announcements>? AnnouncementsList { get; set; }
        public List<Homework>? HomeworkList { get; set; }
    }
}
