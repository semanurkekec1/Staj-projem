namespace Classroom.Models
{
    public class Homework
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime DueDate { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser?ApplicationUser { get; set; }
        public int ClassRoomId { get; set; }
        public ClassRoom? ClassRoom { get; set; }
        public bool IsDelete { get; set; }
        public List<Homework_User>? HomeworkUserList { get; set; }
    }
}
