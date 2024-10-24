using Classroom.Data;
using Classroom.Models;
using Classroom.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Linq;
using Classroom.ViewModels;
using System.Security.Claims;


namespace Classroom.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private ApplicationDbContext db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            this.db = db;
        }

        public IActionResult Index()
        {
            //if(User.Identity.IsAuthenticated)
            //{
            //    return View();
            //}

            //return Redirect("Identity/Account/Login");

            /*
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Kullan�c� ID'si ile istedi�iniz i�lemi yapabilirsiniz


            var ClassRooms = db.ClassRoom.Where(i => i.IsActive && !i.IsDelete).Include(x=>x.ApplicationUser).ToList();
            return View(ClassRooms);
            */

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userClassRooms = db.ClassUser
                .Where(cu => cu.ApplicationUserId == userId && !cu.IsDelete)
                .Include(cu => cu.ClassRoom)
                .ThenInclude(cr => cr.ApplicationUser)
                .Select(cu => cu.ClassRoom)
                .Where(cr => cr.IsActive && !cr.IsDelete)
                .ToList();

            return View(userClassRooms);
        }

        public IActionResult Teacher()
        {
            // ayni home indexi ile siralama yapilabilir degisik bisey yok
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userClassRooms = db.ClassUser
                .Where(cu => cu.ApplicationUserId == userId && cu.Roles && !cu.IsDelete)
                .Include(cu => cu.ClassRoom)
                .ThenInclude(cr => cr.ApplicationUser)
                .Select(cu => cu.ClassRoom)
                .Where(cr => cr.IsActive && !cr.IsDelete)
                .ToList();

            return View("Index",userClassRooms);
        }

        public IActionResult Student()
        {
            // ayni home indexi ile siralama yapilabilir degisik bisey yok
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userClassRooms = db.ClassUser
                .Where(cu => cu.ApplicationUserId == userId && !cu.Roles && !cu.IsDelete)
                .Include(cu => cu.ClassRoom)
                .ThenInclude(cr => cr.ApplicationUser)
                .Select(cu => cu.ClassRoom)
                .Where(cr => cr.IsActive && !cr.IsDelete)
                .ToList();

            return View("Index", userClassRooms);
        }

        public IActionResult Archived()
        {
            // ayni home indexi ile siralama yapilabilir degisik bisey yok
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userClassRooms = db.ClassUser
                .Where(cu => cu.ApplicationUserId == userId && !cu.IsDelete)
                .Include(cu => cu.ClassRoom)
                .ThenInclude(cr => cr.ApplicationUser)
                .Select(cu => cu.ClassRoom)
                .Where(cr => !cr.IsActive && !cr.IsDelete)
                .ToList();

            return View(userClassRooms);
        }

        [HttpGet]
        public IActionResult JoinClassRoom()
        {
            var appUser = db.Users.FirstOrDefault(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));
            ViewBag.ApplicationUser = appUser;
            return View();
        }

        [HttpPost]
        public IActionResult JoinClassRoom(JoinClassRoomModel model)
        {
            var room = db.ClassRoom.FirstOrDefault(c => c.UnicCode == model.ClassRoomUnicCode);

            if (room == null)
            {
                var appUser = db.Users.FirstOrDefault(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));
                ViewBag.ApplicationUser = appUser;
                // E�er s�n�f yoksa, kullan�c�ya hata mesaj� g�sterin veya ba�ka bir i�lem yap�n.
                ModelState.AddModelError(string.Empty, "Bu koda sahip bir s�n�f bulunamad�.");
                return View(model);
            }

            var IsRegister = db.ClassUser.FirstOrDefault(c => c.ClassRoomId == room.Id && c.ApplicationUserId == User.FindFirstValue(ClaimTypes.NameIdentifier));

            if(IsRegister != null)
            {
                var appUser = db.Users.FirstOrDefault(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));
                ViewBag.ApplicationUser = appUser;
                // E�er s�n�fa zaten uye ise
                ModelState.AddModelError(string.Empty, "Bu s�n�fa zaten �yenisiniz.");
                return View(model);
            }

            db.ClassUser.Add(new Class_User
            {
                ClassRoomId = db.ClassRoom.FirstOrDefault(c => c.UnicCode == model.ClassRoomUnicCode).Id,
                ApplicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                Roles = false
            });
            db.SaveChanges();
            return RedirectToAction("Index");
            
        }

        [HttpGet]
        public IActionResult CreateClassRoom()
        {
            return View();
        }


        [HttpPost]
        public IActionResult CreateClassRoom(ClassRoom model)
        {
            if (model.Name == null || model.Description == null)
            {
                return View(model);
            }
            string code = string.Empty;
            var random = new Random();

            // Unik kod olu�turma
            while (code.Length < 7)
            {
                int randomInt = 48 + random.Next(43); // 48-90 aras�nda bir say� �retir

                if ((randomInt >= 48 && randomInt <= 57) || (randomInt >= 65 && randomInt <= 90))
                {
                    char randomChar = (char)randomInt;
                    code += randomChar;
                }
            }

            // Renk kodlar�
            string[] colorCodes = {
                "#6c757d", // Gri
                "#6f42c0", // Mor
                "#fd7e14", // Turuncu
                "#28a745", // Ye�il
                "#007bff", // Mavi
                "#e84393", // Pembe (Pastel tonlar i�in)
                "#f4a261", // Turuncu (Daha s�cak tonlar i�in)
                "#3498db", // Mavi (Daha a��k tonlar i�in)
                "#9b59b6", // Mor (Daha koyu tonlar i�in)
                "#1abc9c"  // Turkuaz (Canl� ve ferahlat�c�)
            };
            string randomColor = colorCodes[random.Next(colorCodes.Length)];

            model.ApplicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            model.IsActive = true;
            model.IsDelete = false;
            model.UnicCode = code;
            model.Color = randomColor; // Modelin Color �zelli�ini rastgele renk kodu ile ayarla
            db.ClassRoom.Add(model);
            db.SaveChanges();

            int id = db.ClassRoom.FirstOrDefault(c => c.UnicCode == code).Id;

            db.ClassUser.Add(new Class_User { ApplicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier), ClassRoomId = id, Roles = true });
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
