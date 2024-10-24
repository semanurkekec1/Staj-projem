using Classroom.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Classroom.Models;

namespace Classroom.Controllers
{
    public class CommentController : Controller
    {
        private readonly ApplicationDbContext db;

        public CommentController(ApplicationDbContext db)
        {
            this.db = db;
        }
        public IActionResult AddComment(int AnnouncementId, int ClassroomId, string description) 
        {
            if (AnnouncementId == null || ClassroomId == null || description == null)
            {
                return NotFound();
            }

            if (db.Announcements.Any(a => a.Id == AnnouncementId && a.IsDelete))
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!db.ClassUser.Any(cu => cu.ApplicationUserId == userId && cu.ClassRoomId == ClassroomId)) 
            { 
                return Forbid();
            }

            db.Comment.Add(new Comment
            {
                Description = description,
                CreatedAt = DateTime.Now,
                AnnouncementsId = AnnouncementId,
                ApplicationUserId = userId
            });
            db.SaveChanges();

            return RedirectToAction("Index", "Classroom", new { id = ClassroomId });
        }

        public IActionResult RemoveComment(int AnnouncementId, int ClassroomId, int CommentId)
        {
            if (AnnouncementId == null || ClassroomId == null || CommentId == null)
            {
                return NotFound();
            }

            // user classta mi
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!db.ClassUser.Any(cu => cu.ApplicationUserId == userId && cu.ClassRoomId == ClassroomId))
            {
                return Forbid();
            }

            if (!db.Announcements.Any(a => a.Id == AnnouncementId && !a.IsDelete))
            {
                return NotFound();
            }

            if (!db.Comment.Any(c => c.Id == CommentId && !c.IsDelete))
            {
                return NotFound();
            }

            var removeComment = db.Comment.FirstOrDefault(c => c.Id == CommentId && c.AnnouncementsId == AnnouncementId);
            removeComment.IsDelete = true;
            db.Comment.Update(removeComment);
            db.SaveChanges();

            return RedirectToAction("Index", "Classroom", new { id = ClassroomId });
        }
    }
}
