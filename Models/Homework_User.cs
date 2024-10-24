using Classroom.Models;

namespace Classroom.Models
{
    public class Homework_User
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }

        public int HomeworkId { get; set; }
        public Homework? Homework { get; set; }
        public string? Work {  get; set; }
        public int Point {  get; set; }
        public DateTime CreatedAt { get; set; }


    }
}
