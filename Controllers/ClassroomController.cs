using Microsoft.AspNetCore.Mvc;
using Classroom.Models;
using Classroom.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Classroom.Controllers
{
    [Authorize]
    public class ClassroomController : Controller
    {
        private readonly ApplicationDbContext db;

        public ClassroomController(ApplicationDbContext db)
        {
            this.db = db;
        }

        public IActionResult Index(int? id)
        {
            // id kontrolu
            if (id == null)
            {
                return NotFound();
            }

            // bu user bu class mi mi
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isUserInClassroom = db.ClassUser.Any(cu => cu.ClassRoomId == id && cu.ApplicationUserId == userId);
            if (!isUserInClassroom)
            {
                return Forbid();
            }

            // boyle bi sinif var mi
            var selectedClassroom = db.ClassRoom.Include(c => c.ApplicationUser).FirstOrDefault(x => x.Id == id && x.IsActive && !x.IsDelete);
            if (selectedClassroom == null)
            {
                return NotFound();
            }

            ViewBag.ApplicationUserRole = db.ClassUser
                .FirstOrDefault(cu => cu.ApplicationUserId == userId && cu.ClassRoomId == id).Roles; //true ogretmen, false ogrenci

            ViewBag.ApplicationUser = User.FindFirstValue(ClaimTypes.NameIdentifier);

            ViewBag.Teachers = db.ClassUser
                .Where(cu => cu.ClassRoomId == id && cu.Roles && !cu.IsDelete)
                .Include(cu => cu.ClassRoom)
                .ThenInclude(cr => cr.ApplicationUser)
                .Select(cu => cu.ApplicationUser)
                .ToList();

            ViewBag.Students = db.ClassUser
                .Where(cu => cu.ClassRoomId == id && !cu.Roles && !cu.IsDelete)
                .Include(cu => cu.ClassRoom)
                .ThenInclude(cr => cr.ApplicationUser)
                .Select(cu => cu.ApplicationUser)
                .ToList();

            ViewBag.Announcements = db.Announcements
                .Where(a => a.ClassRoomId == id && !a.IsDelete)
                .Include(a => a.ApplicationUser)
                .OrderByDescending(a => a.Id) // Duyuruları duyuru ID'sine göre azalan şekilde sıralar
                .ToList();


            ViewBag.Homeworks = db.Homework
                .Where(a => a.ClassRoomId == id)
                .OrderByDescending(a => a.Id) // Duyuruları duyuru ID'sine göre azalan şekilde sıralar
                .ToList();

            ViewBag.Comments = db.Comment
                .Include(c => c.Announcements)
                .Where(c => c.Announcements.ClassRoomId == id && !c.IsDelete)
                .OrderBy(a => a.Id) // Duyuruları duyuru ID'sine göre azalan şekilde sıralar
                .ToList();

            var allHomeworks = db.Homework
                   .Where(hw => hw.ClassRoomId == id && !hw.IsDelete)
                   .ToList();

            var nearestHomework = allHomeworks
                .OrderBy(hw => Math.Abs((hw.DueDate - DateTime.Now).Ticks)) // Order by the absolute difference in ticks
                .FirstOrDefault();

            ViewBag.NearestHomework = nearestHomework;

            return View(selectedClassroom);
        }


        public IActionResult Archived(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var isUserInClassroom = db.ClassUser.Any(cu => cu.ClassRoomId == id && cu.ApplicationUserId == userId);

            if (!isUserInClassroom)
            {
                return Forbid();
            }

            var selectedClassroom = db.ClassRoom.Include(c => c.ApplicationUser).FirstOrDefault(x => x.Id == id && !x.IsActive && !x.IsDelete);

            if (selectedClassroom == null)
            {
                return NotFound();
            }

            ViewBag.ApplicationUserRole = db.ClassUser
                .FirstOrDefault(cu => cu.ApplicationUserId == userId && cu.ClassRoomId == id).Roles; //true ogretmen, false ogrenci

            ViewBag.ApplicationUser = User.FindFirstValue(ClaimTypes.NameIdentifier);

            ViewBag.Teachers = db.ClassUser
                .Where(cu => cu.ClassRoomId == id && cu.Roles)
                .Include(cu => cu.ClassRoom)
                .ThenInclude(cr => cr.ApplicationUser)
                .Select(cu => cu.ApplicationUser)
                .ToList();

            ViewBag.Students = db.ClassUser
                .Where(cu => cu.ClassRoomId == id && !cu.Roles)
                .Include(cu => cu.ClassRoom)
                .ThenInclude(cr => cr.ApplicationUser)
                .Select(cu => cu.ApplicationUser)
                .ToList();

            ViewBag.Announcements = db.Announcements
                .Where(a => a.ClassRoomId == id && !a.IsDelete)
                .Include(a => a.ApplicationUser)
                .OrderByDescending(a => a.Id) // Duyuruları duyuru ID'sine göre azalan şekilde sıralar
                .ToList();

            ViewBag.Homeworks = db.Homework
                .Where(a => a.ClassRoomId == id)
                .OrderByDescending(a => a.Id) // Duyuruları duyuru ID'sine göre azalan şekilde sıralar
                .ToList();

            ViewBag.Comments = db.Comment
                .Include(c => c.Announcements)
                .Where(c => c.Announcements.ClassRoomId == id && !c.IsDelete)
                .OrderBy(a => a.Id) // Duyuruları duyuru ID'sine göre azalan şekilde sıralar
                .ToList();


            return View("Index", selectedClassroom);
        }

        public IActionResult Announcements(string announcements, int classroomId)
        {
            db.Announcements.Add(new Announcements
            {
                Contents = announcements,
                CreatedAt = DateTime.Now,
                ApplicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                ClassRoomId = classroomId,
                IsDelete = false
            });
            db.SaveChanges();

            return RedirectToAction("Index", new { id = classroomId });
        }

        public IActionResult MakeTeacher(string? userId, int? classroomId)
        {
            if (userId == null || classroomId == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var control = db.ClassUser.Any(cu => cu.ClassRoomId == classroomId && cu.ApplicationUserId == currentUserId && cu.Roles);
            if (!control)
            {
                return Forbid();
            }

            var classUser = db.ClassUser.FirstOrDefault(cu => cu.ApplicationUserId == userId && cu.ClassRoomId == classroomId);
            classUser.Roles = true;
            db.ClassUser.Update(classUser); // Mark the entity as modified
            db.SaveChanges();

            return RedirectToAction("Index", new { id = classroomId });
        }

        public IActionResult RemoveStudent(string? userId, int? classroomId)
        {
            // null kontrol
            if (userId == null || classroomId == null)
            {
                return NotFound();
            }
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var control = db.ClassUser.Any(cu => cu.ClassRoomId == classroomId && cu.ApplicationUserId == currentUserId && cu.Roles);
            if (!control)
            {
                return Forbid();
            }

            // siniftan silme
            var cu = db.ClassUser.FirstOrDefault(cu => cu.ApplicationUserId == userId && cu.ClassRoomId == classroomId && !cu.IsDelete);
            cu.IsDelete = true; 
            db.ClassUser.Update(cu);
            db.SaveChanges();

            return RedirectToAction("Index", new { id = classroomId });

        }
        public IActionResult ArchivedClassroom(int? id)
        {
            // classroom id kontrol
            if (id == null)
            {
                return NotFound();
            }

            // user sinifta var mi ve rolu ogretmen mi kontrol
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var control = db.ClassUser.Any(cu => cu.ClassRoomId == id && cu.ApplicationUserId == userId && cu.Roles);
            if (!control)
            {
                return Forbid();
            }

            var Classroom = db.ClassRoom.FirstOrDefault(c => c.Id == id);
            Classroom.IsActive = false;
            db.ClassRoom.Update(Classroom);
            db.SaveChanges();

            return RedirectToAction("Index", "Home");
        }
        public IActionResult DeleteClassroom(int? id)
        {
            // classroom id kontrol
            if (id == null)
            {
                return NotFound();
            }

            // user sinifta var mi ve rolu ogretmen mi kontrol
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var control = db.ClassUser.Any(cu => cu.ClassRoomId == id && cu.ApplicationUserId == userId && cu.Roles);
            if (!control)
            {
                return Forbid();
            }

            var Classroom = db.ClassRoom.FirstOrDefault(c => c.Id == id);
            Classroom.IsDelete = true;
            db.ClassRoom.Update(Classroom);
            db.SaveChanges();

            return RedirectToAction("Index", "Home");
        }

        public IActionResult UnarchivedClassroom(int? id)
        {
            // classroom id kontrol
            if (id == null)
            {
                return NotFound();
            }

            // user sinifta var mi ve rolu ogretmen mi kontrol
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var control = db.ClassUser.Any(cu => cu.ClassRoomId == id && cu.ApplicationUserId == userId && cu.Roles);
            if (!control)
            {
                return Forbid();
            }

            var Classroom = db.ClassRoom.FirstOrDefault(c => c.Id == id);
            Classroom.IsActive = true;
            db.ClassRoom.Update(Classroom);
            db.SaveChanges();

            return RedirectToAction("Index", "Home");
        }

        public IActionResult UnenrollClassroom(int? id)
        {
            // classroom id kontrol
            if (id == null)
            {
                return NotFound();
            }

            // user sinifta var mi
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var control = db.ClassUser.Any(cu => cu.ClassRoomId == id && cu.ApplicationUserId == userId);
            if (!control)
            {
                return Forbid();
            }
            
            var ClassUser = db.ClassUser.FirstOrDefault(cu => cu.ClassRoomId == id && cu.ApplicationUserId == userId);
            ClassUser.IsDelete = true;
            db.ClassUser.Update(ClassUser);
            db.SaveChanges();
            
            return RedirectToAction("Index", "Home");
        }

        public IActionResult DeleteAnnonucements(int? classroomId, int announcementsId)
        {
            if (classroomId == null || announcementsId == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(!(db.Announcements.Any(a => a.ApplicationUserId == currentUserId && a.Id == announcementsId) || db.ClassUser.Any(cu => cu.ApplicationUserId == currentUserId && cu.ClassRoomId == classroomId && cu.Roles)))
            {
                return NotFound();
            }

            var announcements = db.Announcements.FirstOrDefault(a => a.ClassRoomId == classroomId && a.Id == announcementsId);
            announcements.IsDelete = true;
            db.Announcements.Update(announcements);
            db.SaveChanges();

            return RedirectToAction("Index", new { id = classroomId });
        }
    }
}
