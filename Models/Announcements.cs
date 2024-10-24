using Classroom.Models;

namespace Classroom.Models
{
    public class Announcements
    {
        public int Id { get; set; }
        public string? Contents { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
        public int ClassRoomId { get; set; }
        public ClassRoom? ClassRoom { get; set; }
        public bool IsDelete { get; set; }
    }
}
