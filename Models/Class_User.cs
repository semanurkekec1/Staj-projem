using Classroom.Models;

namespace Classroom.Models
{
    public class Class_User
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }

        public int ClassRoomId { get; set; }
        public ClassRoom? ClassRoom { get; set; }
        public bool Roles { get; set; } // true ise ogretmen false ise  ogerenci
        public bool IsDelete { get; set; }
    }
}
