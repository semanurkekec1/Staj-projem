using Classroom.Data;
using Classroom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Intrinsics.Arm;
using System.Security.Claims;

namespace Classroom.Controllers
{
    [Authorize]
    public class HomeworkController : Controller
    {
        private readonly ApplicationDbContext db;

        public HomeworkController(ApplicationDbContext db)
        {
            this.db = db;
        }
        public IActionResult Index(int? HomeworkId)
        {
            if (HomeworkId == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var isUserInClassroom = db.HomeworkUser.Any(cu => cu.HomeworkId == HomeworkId && cu.ApplicationUserId == userId);

            if (!isUserInClassroom)
            {
                return Forbid();
            }

            var selectedHomework = db.Homework.Include(h => h.ApplicationUser).Include(h => h.ClassRoom).FirstOrDefault(x => x.Id == HomeworkId && !x.IsDelete);

            if (selectedHomework == null)
            {
                return NotFound();
            }
            ViewBag.HomeworkUser = db.HomeworkUser.FirstOrDefault(h => h.ApplicationUserId == userId && h.HomeworkId == HomeworkId);

            return View(selectedHomework);
        }

        [HttpGet]
        public IActionResult TeachIndex(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!db.ClassUser.Any(cu => cu.ApplicationUserId == userId && cu.Roles))
            {
                return Forbid();
            }

            var selectedHomework = db.Homework.Include(h => h.ApplicationUser).FirstOrDefault(x => x.Id == id && !x.IsDelete);
            if (selectedHomework == null)
            {
                return NotFound();
            }

            return View(selectedHomework);
        }

        [HttpPost]
        public IActionResult TeachIndex(string? questionTitle, string? description, DateTime dueDateTime, int homeworkId)
        {
            // Check for invalid input
            if (string.IsNullOrEmpty(questionTitle) || string.IsNullOrEmpty(description) || homeworkId == null)
            {
                return BadRequest("Invalid input.");
            }

            // Check if the homework exists
            var hw = db.Homework.FirstOrDefault(h => h.Id == homeworkId && !h.IsDelete);
            if (hw == null)
            {
                return NotFound();
            }

            if(!db.ClassUser.Any(cu => cu.ApplicationUserId == User.FindFirstValue(ClaimTypes.NameIdentifier) && cu.Roles))
            {
                return Forbid();
            }

            var Hw = db.Homework.FirstOrDefault(h => h.Id == homeworkId && !h.IsDelete);

            Hw.Name = questionTitle;
            Hw.Description = description;
            Hw.DueDate = dueDateTime;
            db.Homework.Update(hw); // Mark the entity as modified
            db.SaveChanges();

            return RedirectToAction("TeachIndex", new { id = Hw.Id });
        }

        [HttpGet]
        public IActionResult CreateHomework(int? id)
        {
            // id degeri girildi mi
            if (id == null)
            {
                return NotFound();
            }

            // Bu kullanici bu sinifta var mi eger var ise rolu ogretmen mi
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isUserInClassroom = db.ClassUser.Any(cu => cu.ClassRoomId == id && cu.ApplicationUserId == userId && cu.Roles);
            if (!isUserInClassroom)
            {
                return Forbid();
            }

            // boyle bi sinif var mi
            var selectedClassroom = db.ClassRoom.FirstOrDefault(c => c.Id == id && c.IsActive && !c.IsDelete);
			if (selectedClassroom == null)
            {
                return NotFound();
            }

            return View(selectedClassroom);
        }


        [HttpPost]
        public IActionResult CreateHomework(string questionTitle, string description, DateTime dueDateTime, int classroomId)
        {
            // id degeri girildi mi
            if (classroomId == 0)
            {
                return NotFound();
            }

            // Bu kullanici bu sinifta var mi eger var ise rolu ogretmen mi
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isUserInClassroom = db.ClassUser.Any(cu => cu.ClassRoomId == classroomId && cu.ApplicationUserId == userId && cu.Roles);
            if (!isUserInClassroom)
            {
                return Forbid();
            }

            // Boyle bir sinif var mi
            var selectedClassroom = db.ClassRoom.Any(c => c.Id == classroomId && c.IsActive && !c.IsDelete);
            if (!selectedClassroom)
            {
                return NotFound();
            }

            // Yeni Homework ekleniyor
            var newHomework = new Homework
            {
                Name = questionTitle,
                Description = description,
                ApplicationUserId = userId,
                CreatedAt = DateTime.Now,
                IsDelete = false,
                ClassRoomId = classroomId,
                DueDate = dueDateTime
            };

            db.Homework.Add(newHomework);
            db.SaveChanges();  // SaveChanges çağrısı yeni Homework kaydının Id'sini oluşturur

            // HomeworkUser tablosuna kayıtlar ekleniyor
            var UsersList = db.ClassUser.Where(cu => cu.ClassRoomId == classroomId && !cu.IsDelete && !cu.Roles).Select(cu => cu.ApplicationUserId).ToList();

            foreach (var user in UsersList)
            {
                db.HomeworkUser.Add(new Homework_User
                {
                    ApplicationUserId = user,
                    HomeworkId = newHomework.Id,  // Yeni oluşturulan Homework'ün Id'si burada kullanılıyor
                    Point = -1,
                });
            }

            db.SaveChanges();  // HomeworkUser kayıtlarını veritabanına kaydetmek için SaveChanges çağrılıyor

            return RedirectToAction("Index", "Classroom", new { id = classroomId });
        }

        //odev ekleme sayfasi
        [HttpGet]
        public IActionResult AddHomework(int? HomeworkId)
        {
            if (HomeworkId == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var selectedHomework = db.Homework
                .Include(h => h.ClassRoom)
                .Include(h => h.ApplicationUser)
                .FirstOrDefault(x => x.Id == HomeworkId && !x.IsDelete);

            if (selectedHomework == null)
            {
                return NotFound();
            }

            // class user esleseme
            if (!db.ClassUser.Any(cu => cu.ApplicationUserId == userId && cu.ClassRoomId == selectedHomework.ClassRoom.Id && !cu.IsDelete))
            {
                return Forbid();
            }

            if (!db.HomeworkUser.Any(hu => hu.ApplicationUserId == userId && hu.HomeworkId == HomeworkId))
            {
                return Forbid();
            }

            if(selectedHomework.DueDate < DateTime.Now)
            {
                return Forbid();
            }

            return View(selectedHomework);
        }

        // ogreninin odev ekleme yapmasi
        [HttpPost]
        public IActionResult AddHomework(int HomeworkId, int ClassroomId, string HwText) {
            if (HomeworkId == null || ClassroomId == null || HwText == null)
            {
                return NotFound();
            }

            // Bu kullanici bu sinifta var mi eger var ise rolu ogrenci mi
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isUserInClassroom = db.ClassUser.Any(cu => cu.ClassRoomId == ClassroomId && cu.ApplicationUserId == userId && !cu.Roles);
            if (!isUserInClassroom)
            {
                return Forbid();
            }

            if(!db.HomeworkUser.Any(hu => hu.HomeworkId == HomeworkId && hu.ApplicationUserId == userId))
            {
                return Forbid();
            }

            if(db.Homework.FirstOrDefault(h => h.Id == HomeworkId).DueDate < DateTime.Now)
            {
                return Forbid();
            }

            var selectedHw = db.HomeworkUser.FirstOrDefault(hu => hu.ApplicationUserId == userId && hu.HomeworkId == HomeworkId);
            selectedHw.Work = HwText;
            selectedHw.CreatedAt = DateTime.Now;
            db.HomeworkUser.Update(selectedHw);
            db.SaveChanges();

            return RedirectToAction("Index", "Homework", new { HomeworkId = HomeworkId });
        }

        public IActionResult HomeworkList(int HomeworkId, int ClassroomId)
        {
            if (HomeworkId == null || ClassroomId == null)
            {
                return NotFound();
            }

            // Bu kullanici bu sinifta var mi eger var ise rolu ogretmen mi
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!db.ClassUser.Any(cu => cu.ClassRoomId == ClassroomId && cu.ApplicationUserId == userId && cu.Roles)) {
                return Forbid();
            }

            //homework classroom kontrrol
            if(!db.Homework.Any(h => h.ClassRoomId == ClassroomId && h.Id == HomeworkId)) {
                return NotFound();
            }

            ViewBag.WorkList = db.HomeworkUser.Include(hu => hu.ApplicationUser).Where(hu => hu.HomeworkId == HomeworkId).ToList();
            var Homework = db.Homework.Include(h => h.ApplicationUser).FirstOrDefault(h => h.Id == HomeworkId);

            return View(Homework);
        }

        public IActionResult HomeworkGrade(int Grade, int ClassRoomId, int HomeworkId, int HomeworkUserId)
        {

            if (HomeworkId == null || ClassRoomId == null || Grade == null || HomeworkUserId == null)
            {
                return NotFound();
            }

            // Bu kullanici bu sinifta var mi eger var ise rolu ogretmen mi
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!db.ClassUser.Any(cu => cu.ClassRoomId == ClassRoomId && cu.ApplicationUserId == userId && cu.Roles))
            {
                return NotFound();
            }

            //homework classroom kontrrol
            if (!db.Homework.Any(h => h.ClassRoomId == ClassRoomId && h.Id == HomeworkId))
            {
                return NotFound();
            }

            ViewBag.WorkList = db.HomeworkUser.Include(hu => hu.ApplicationUser).Where(hu => hu.HomeworkId == HomeworkId).ToList();

            var HomeworkUser = db.HomeworkUser.FirstOrDefault(hu => hu.Id == HomeworkUserId);
            HomeworkUser.Point = Grade;
            db.HomeworkUser.Update(HomeworkUser);
            db.SaveChanges();

            return RedirectToAction("HomeworkList", "Homework", new { HomeworkId = HomeworkId, ClassroomId = ClassRoomId });
        }

    }
}
