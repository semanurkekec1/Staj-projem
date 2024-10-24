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
            // Kullanýcý ID'si ile istediðiniz iþlemi yapabilirsiniz


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
                // Eðer sýnýf yoksa, kullanýcýya hata mesajý gösterin veya baþka bir iþlem yapýn.
                ModelState.AddModelError(string.Empty, "Bu koda sahip bir sýnýf bulunamadý.");
                return View(model);
            }

            var IsRegister = db.ClassUser.FirstOrDefault(c => c.ClassRoomId == room.Id && c.ApplicationUserId == User.FindFirstValue(ClaimTypes.NameIdentifier));

            if(IsRegister != null)
            {
                var appUser = db.Users.FirstOrDefault(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));
                ViewBag.ApplicationUser = appUser;
                // Eðer sýnýfa zaten uye ise
                ModelState.AddModelError(string.Empty, "Bu sýnýfa zaten üyenisiniz.");
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

            // Unik kod oluþturma
            while (code.Length < 7)
            {
                int randomInt = 48 + random.Next(43); // 48-90 arasýnda bir sayý üretir

                if ((randomInt >= 48 && randomInt <= 57) || (randomInt >= 65 && randomInt <= 90))
                {
                    char randomChar = (char)randomInt;
                    code += randomChar;
                }
            }

            // Renk kodlarý
            string[] colorCodes = {
                "#6c757d", // Gri
                "#6f42c0", // Mor
                "#fd7e14", // Turuncu
                "#28a745", // Yeþil
                "#007bff", // Mavi
                "#e84393", // Pembe (Pastel tonlar için)
                "#f4a261", // Turuncu (Daha sýcak tonlar için)
                "#3498db", // Mavi (Daha açýk tonlar için)
                "#9b59b6", // Mor (Daha koyu tonlar için)
                "#1abc9c"  // Turkuaz (Canlý ve ferahlatýcý)
            };
            string randomColor = colorCodes[random.Next(colorCodes.Length)];

            model.ApplicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            model.IsActive = true;
            model.IsDelete = false;
            model.UnicCode = code;
            model.Color = randomColor; // Modelin Color özelliðini rastgele renk kodu ile ayarla
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
