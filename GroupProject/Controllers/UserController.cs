using GroupProject.Constants;
using GroupProject.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GroupProject.Controllers
{
    [Authorize(AuthenticationSchemes = "Fitness247UserAuthentication")]
    public class UserController:Controller
    {
        private DB_Context db_context;

        public UserController(DB_Context db_context)
        {
            this.db_context = db_context;
        }

        public IActionResult Index(long locationID = 1)
        {
            User loggedInuser = db_context.Users.Where(x => x.Email == HttpContext.User.Identity.Name || x.UserName == HttpContext.User.Identity.Name).ToList().First();
            ViewBag.User = loggedInuser;
            ViewBag.Location = db_context.Locations.Where(x => x.LocationID == locationID).AsNoTracking().Single();
            return View();
        }

        //[AllowAnonymous]
        //public IActionResult Login(string requestPath)
        //{
        //    ViewBag.RequestPath = requestPath ?? "/user";
        //    return Index();
        //}
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!IsAuthentic(model.Username, model.Password))
            {
                RedirectToActionResult redirect = new RedirectToActionResult("Index", "Home", new { Error = "Invalid Credentials" });
                return redirect;
            }

            // create claims
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.Username)
            };

            // create identity
            ClaimsIdentity identity = new ClaimsIdentity(claims, "cookie");

            // create principal
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            // sign-in
            await HttpContext.SignInAsync(
                    scheme: "Fitness247UserAuthentication",
                    principal: principal,
                    properties: new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe, // for 'remember me' feature
                        ExpiresUtc = DateTime.UtcNow.AddMinutes(30)
                    });

            return Redirect(model.RequestPath ?? "/user");
        }
        public async Task<IActionResult> Logout(string requestPath)
        {
            await HttpContext.SignOutAsync(
                    scheme: "Fitness247UserAuthentication");

            return Redirect("/");
        }
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Register(RegistrationModel model)
        {

            User u = new User()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.UserName,
                Email = model.Email,
                DateOfBirth = model.DateOfBirth,
                Phone = model.Phone,
                Password = model.Password,
                _UserRole = UserRoles.Member,
                AddressLine1 = " ",
                AddressLine2 = " ",
                _Gender = model._Gender,

                
            };
            db_context.Users.Add(u);
            db_context.SaveChanges();
            RedirectToActionResult redirect = new RedirectToActionResult("Index", "Home", new { RegisterMessage = "Registration Complete" });
            return redirect;
        }

        private bool IsAuthentic(string username, string password)
        {
            int count = db_context.Users.Count(x => x._UserRole < UserRoles.Instructor && (x.Email == username || x.UserName == username) && x.Password == password);
            if (count == 1)
                return true;
            return false;
        }

        public PartialViewResult BookingsCalendar(long locationID,string startDate,string endDate,int week=0)
        {
            User loggedInuser = db_context.Users.Where(x => x.Email == HttpContext.User.Identity.Name || x.UserName == HttpContext.User.Identity.Name).ToList().First();

            ViewBag.Week = week;
            var sDate = DateTime.Parse(startDate).Date;
            var eDate = DateTime.Parse(endDate).Date;
            List<Class> classes = db_context.Classes.Include(x=>x.SubstituteInstructor).Include(c=>c.ClassType).ThenInclude(x=>x.Instructor).Where(x => x.ClassType.LocationID == locationID && x.StartDate >= sDate && x.EndDate <= eDate).AsNoTracking().ToList<Class>();
            ViewBag.Classes = classes;

            List<Location> locations = db_context.Locations.AsNoTracking().ToList();
            ViewBag.Locations = locations;

            List<Instructor> instructors = db_context.Instructors.AsNoTracking().ToList();
            ViewBag.Instructors = instructors;

            ViewBag.ClassTypes = db_context.ClassTypes.Include(x => x.Location).Where(x => x.LocationID == locationID).AsNoTracking().ToList();

            ViewBag.UserReservations = db_context.Reservations.AsNoTracking().Where(x => x.UserID == loggedInuser.UserID).ToList();

            Location l= db_context.Locations.Where(x => x.LocationID == locationID).AsNoTracking().Single();
            ViewBag.LocationDetails = l;
            ViewBag.Today=DateTime.UtcNow.ToTimeZoneTime(l.TimeZone);
            Dictionary<string,string> dateRange = new Dictionary<string,string>();
            for (DateTime date = sDate; date.Date <= eDate; date = date.AddDays(1))
            {
                dateRange.Add(date.ToString("dddd"),date.ToString("yyyy-MM-dd"));
            }
            foreach(Class c in classes)
            {
                DateTime today= DateTime.UtcNow.ToTimeZoneTime(l.TimeZone);
                DateTime classDate = new DateTime(c.StartDate.Year, c.StartDate.Month, c.StartDate.Day, c.StartTime.Hours, c.StartTime.Minutes, c.StartTime.Seconds);
                TimeSpan diff = classDate.Subtract(today);
                if (diff.Ticks > 0)
                    c.DisabledForView = false;
                else
                    c.DisabledForView = true;

            }

            ViewBag.DateRange = dateRange;
            return PartialView();
        }

        public PartialViewResult EnrollInClass(long locationID, string startDate, string endDate, int week,long ClassID, string message="")
        {
            User loggedInuser = db_context.Users.Where(x => x.Email == HttpContext.User.Identity.Name || x.UserName == HttpContext.User.Identity.Name).ToList().First();
            Class c = db_context.Classes.AsNoTracking().Include(x=>x.ClassSchedule).ThenInclude(x=>x.RoomLayout).Include(x => x.SubstituteInstructor).Include(x => x.ClassType).ThenInclude(x => x.Instructor).Single(x => x.ClassID == ClassID);
            ViewBag.Class = c;
            ViewBag.LocationID = locationID;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.Week = week;
            ViewBag.Message = message;
            ViewBag.ReservationData= db_context.Reservations.AsNoTracking().Where(x=>x.ClassID==ClassID).ToList();
            ViewBag.UserReservation = db_context.Reservations.AsNoTracking().SingleOrDefault(x => x.UserID == loggedInuser.UserID && x.ClassID == ClassID);
            return PartialView();
        }

        [HttpPost]
        public JsonResult EnrollInClass(long locationID,long classID, long roomLayoutID, int seatNumber,int reservationStatus)
        {
            //	1 - Enrolled
            //	2 - Cancelled
            //	3 - Maintenance
            //	4 - LateCancelled
            //  9 - WaitListed
            User loggedInuser = db_context.Users.AsNoTracking().Where(x => x.Email == HttpContext.User.Identity.Name || x.UserName == HttpContext.User.Identity.Name).ToList().First();
            Location l = db_context.Locations.AsNoTracking().Single(x => x.LocationID == locationID);
            if(db_context.Reservations.Where(x=>x.ClassID==classID && x.UserID==loggedInuser.UserID).Count()>0)
            {
                var result = new
                {
                    StatusCode = "Error",
                    Message="You're already enrolled"
                };
                return Json(result);
            }
            else
            {
                if (roomLayoutID == 0)
                {
                    Reservation r = new Reservation();
                    r.UserID = loggedInuser.UserID;
                    r.RoomLayoutID = roomLayoutID;
                    r.ClassID = classID;
                    r.ReservationDate= DateTime.UtcNow.ToTimeZoneTime(l.TimeZone);
                    r.SeatNumber = db_context.Reservations.AsNoTracking().Where(x => x.ClassID == classID && x.RoomLayoutID == roomLayoutID).Count() + 1;
                    r.ReservationStatus = 1;
                    r.SignedIn = 0;

                    db_context.Add(r);
                    db_context.SaveChanges();
                    var result = new
                    {
                        StatusCode = "Success",
                        Message = "Successfully enrolled in the class"
                    };
                    return Json(result);
                }
                else
                {
                    if(db_context.Reservations.Where(x => x.ClassID == classID && x.RoomLayoutID == roomLayoutID && x.SeatNumber==seatNumber).Count() > 0)
                    {
                        var result = new
                        {
                            StatusCode = "Error",
                            Message = "Someone already reserved that seat"
                        };
                        return Json(result);
                    }
                    else
                    {
                        Reservation r = new Reservation();
                        r.UserID = loggedInuser.UserID;
                        r.RoomLayoutID = roomLayoutID;
                        r.ClassID = classID;
                        r.ReservationDate = DateTime.UtcNow.ToTimeZoneTime(l.TimeZone);
                        r.SeatNumber = seatNumber;
                        r.ReservationStatus = 1;
                        r.SignedIn = 0;
                        db_context.Add(r);
                        db_context.SaveChanges();
                        var result = new
                        {
                            StatusCode = "Success",
                            Message = "Successfully enrolled in the class"
                        };
                        return Json(result);
                    }
                    
                }
            }
        }

        public PartialViewResult MyReservations()
        {
            return PartialView();
        }

        [HttpPost]
        public JsonResult GetMyReservations()
        {
            User loggedInuser = db_context.Users.AsNoTracking().Where(x => x.Email == HttpContext.User.Identity.Name || x.UserName == HttpContext.User.Identity.Name).ToList().First();
            var searchParam = HttpContext.Request.Form["search[value]"];
            var start = HttpContext.Request.Form["start"][0];
            var length = HttpContext.Request.Form["length"][0];
            var totalCount = db_context.Reservations.Where(x => x.UserID == loggedInuser.UserID).Count();
            var filterCount = db_context.Reservations.Where(x => x.UserID == loggedInuser.UserID).Count();
            List<dynamic> filteredReservations = db_context.Reservations.Include(c => c.Class).ThenInclude(c => c.ClassType).ThenInclude(x=>x.Instructor).AsNoTracking().Where(x=>x.UserID==loggedInuser.UserID).Skip(int.Parse(start)).Take(int.Parse(length)).ToList<dynamic>();

            if (searchParam[0].Trim() != "")
            {
                filteredReservations = db_context.Reservations.Include(c => c.Class).ThenInclude(c => c.ClassType).ThenInclude(x => x.Instructor).AsNoTracking().Where(x => x.UserID == loggedInuser.UserID && x.Class.ClassType.ClassName.Contains(searchParam[0])).Skip(int.Parse(start)).Take(int.Parse(length)).ToList<dynamic>();
                filterCount = filteredReservations.Count();
            }

            dynamic data = new
            {
                draw = HttpContext.Request.Form["draw"],
                recordsTotal = totalCount,
                recordsFiltered = filterCount,
                data = filteredReservations
            };
            return Json(data);
        }

        public PartialViewResult MyPurchases()
        {
            return PartialView();
        }

        [HttpPost]
        public JsonResult GetClasses()
        {
            var searchParam = HttpContext.Request.Form["search[value]"];
            var start = HttpContext.Request.Form["start"][0];
            var length = HttpContext.Request.Form["length"][0];
            var totalCount = db_context.Classes.Count();
            var filterCount = db_context.Classes.Count();
            List<dynamic> filteredClasses = db_context.Classes.Include(c => c.ClassType).ThenInclude(c => c.Location).Include(r => r.ClassSchedule).AsNoTracking().Skip(int.Parse(start)).Take(int.Parse(length)).ToList<dynamic>();

            if (searchParam[0].Trim() != "")
            {
                filteredClasses = db_context.Classes.Include(c => c.ClassType).ThenInclude(c => c.Location).Include(r => r.ClassSchedule).AsNoTracking().Where(x => x.ClassType.ClassName.Contains(searchParam[0]) || x.ClassSchedule.ScheduleName.Contains(searchParam[0])).Skip(int.Parse(start)).Take(int.Parse(length)).ToList<dynamic>();
                filterCount = filteredClasses.Count();
            }

            dynamic data = new
            {
                draw = HttpContext.Request.Form["draw"],
                recordsTotal = totalCount,
                recordsFiltered = filterCount,
                data = filteredClasses
            };
            return Json(data);
        }
    }
}
