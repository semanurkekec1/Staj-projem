namespace Classroom.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
        public int AnnouncementsId { get; set; }
        public Announcements? Announcements { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDelete {  get; set; }
    }
}
