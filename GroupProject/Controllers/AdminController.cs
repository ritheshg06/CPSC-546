using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GroupProject.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using GroupProject.Constants;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Json;

namespace GroupProject.Controllers
{
    [Authorize(AuthenticationSchemes = "Fitness247AdminAuthentication")]
    public class AdminController : Controller
    {
        private DB_Context db_context;

        public AdminController(DB_Context db_context)
        {
            this.db_context = db_context;
        }

        public IActionResult Index()
        {
            User loggedInuser = db_context.Users.Where(x => x.Email == HttpContext.User.Identity.Name|| x.UserName==HttpContext.User.Identity.Name).ToList().First();
            ViewBag.User = loggedInuser;
            return View();
        }
        [AllowAnonymous]
        public IActionResult Login(string requestPath)
        {
            ViewBag.RequestPath = requestPath ?? "/admin";
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!IsAuthentic(model.Username, model.Password))
                return View();

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
                    scheme: "Fitness247AdminAuthentication",
                    principal: principal,
                    properties: new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe, // for 'remember me' feature
                        ExpiresUtc = DateTime.UtcNow.AddMinutes(30)
                    });

            return Redirect(model.RequestPath ?? "/admin");
        }
        public async Task<IActionResult> Logout(string requestPath)
        {
            await HttpContext.SignOutAsync(
                    scheme: "Fitness247AdminAuthentication");

            return RedirectToAction("Login");
        }

        public PartialViewResult Dashboard()
        {
            return PartialView();
        }
        public PartialViewResult Reservations(long locationID = 1, int week = 0)
        {
            List<Location> Locations= db_context.Locations.AsNoTracking().ToList();
            ViewBag.Locations = Locations;
            Location Location = Locations.Where(x => x.LocationID == locationID).Single();
            ViewBag.Location = Location;
            ViewBag.Week = week;
            ViewBag.Instructors = db_context.Instructors.AsNoTracking().ToList();

            DateTime today = DateTime.UtcNow.ToTimeZoneTime(Location.TimeZone);
            DateTime sDate = DateTimeExtensions.StartOfWeek(DateTime.UtcNow, DayOfWeek.Sunday).AddDays(7*week);
            DateTime eDate = sDate.AddDays(6);
            Dictionary<string, DateTime> dateRange = new Dictionary<string, DateTime>();
            for (DateTime date = sDate; date.Date <= eDate; date = date.AddDays(1))
            {
                //DateTime dt = new DateTime(date.Year, date.Month, date.Day, today.Hour, today.Minute, today.Second, today.Millisecond);
                dateRange.Add(date.ToString("dddd"), date);
            }
            ViewBag.TodayDate = today;
            ViewBag.DateRange = dateRange;

            List<Class> classes = db_context.Classes.Include(x=>x.ClassType).Include(x=>x.ClassSchedule).Include(x=>x.Reservations).AsNoTracking().Where(x => x.ClassType.LocationID == locationID && x.StartDate>=sDate && x.EndDate<=eDate).ToList();
            ViewBag.Classes = classes;

            return PartialView();
        }

        public JsonResult GetClassReservationData(long classID,long roomID=0)
        {
            dynamic data = null;
            if(roomID==0)
            {
                data = new
                {
                    classDetails = db_context.Classes.AsNoTracking().Include(x => x.ClassType).ThenInclude(x => x.Instructor).Include(x => x.SubstituteInstructor).SingleOrDefault(x => x.ClassID == classID),
                    reservationDetails=db_context.Reservations.AsNoTracking().Include(x => x.User).Where(x=>x.ClassID==classID && (x.ReservationStatus==1 || x.ReservationStatus==3))
                };
            }
            else
            {
                data = new
                {
                    classDetails = db_context.Classes.AsNoTracking().Include(x => x.ClassType).ThenInclude(x => x.Instructor).Include(x => x.SubstituteInstructor).SingleOrDefault(x => x.ClassID == classID),
                    reservationDetails = db_context.Reservations.AsNoTracking().Include(x=>x.User).Where(x => x.ClassID == classID && (x.ReservationStatus == 1 || x.ReservationStatus == 3)),
                    roomLayout = db_context.RoomLayouts.AsNoTracking().SingleOrDefault(x => x.RoomLayoutID == roomID)
                };
            }
            
            return Json(data);
        }

        [HttpPost]
        public JsonResult BookingOperation()
        {
            string OperationID = HttpContext.Request.Form["operationID"];
            long ClassID = long.Parse(HttpContext.Request.Form["classID"]);
            long RoomID = long.Parse(HttpContext.Request.Form["roomID"]);
            long locationID = long.Parse(HttpContext.Request.Form["locationID"]);
            Location location = db_context.Locations.AsNoTracking().Single(x => x.LocationID == locationID);
            var result = new { StatusCode = "", Message = ""};
            switch (OperationID)
            {
                case "1":
                    {
                        long userID = long.Parse(HttpContext.Request.Form["userID"]);
                        int SpotNumber = int.Parse(HttpContext.Request.Form["spotNumber"]);
                        if (userID==0)
                        {
                            result = new
                            {
                                StatusCode = "Error",
                                Message = "Please Select a Member"
                            };
                            return Json(result);
                        }
                        else
                        {
                            if (db_context.Reservations.AsNoTracking().Where(x => x.SeatNumber == SpotNumber && x.RoomLayoutID==RoomID && x.ReservationStatus==1).Count() > 0)
                            {
                                result = new
                                {
                                    StatusCode = "Error",
                                    Message = "Seat already reserved"
                                };
                                return Json(result);
                            }
                            if (db_context.Reservations.AsNoTracking().Where(x=>x.ClassID== ClassID && x.RoomLayoutID==RoomID && x.UserID==userID && x.ReservationStatus==1 ).Count()>0)
                            {
                                result = new
                                {
                                    StatusCode = "Error",
                                    Message = "Member already enrolled"
                                };
                                return Json(result);
                            }
                            else
                            {
                                
                                Reservation r = new Reservation();
                                r.UserID = userID;
                                r.RoomLayoutID = RoomID;
                                r.ClassID = ClassID;
                                r.ReservationDate = DateTime.UtcNow.ToTimeZoneTime(location.TimeZone);
                                r.SeatNumber = SpotNumber;
                                r.ReservationStatus = 1;
                                r.SignedIn = 0;
                                db_context.Add(r);
                                db_context.SaveChanges();
                                result = new
                                {
                                    StatusCode = "Success",
                                    Message = "Seat booked successfully"
                                };
                                return Json(result);
                            }
                        }
                    }
                case "2"://Cancel Reservation
                    {
                        int SpotNumber = int.Parse(HttpContext.Request.Form["spotNumber"]);
                        long reservationID = long.Parse(HttpContext.Request.Form["reservationID"]);
                        Reservation r = db_context.Reservations.AsNoTracking().SingleOrDefault(x => x.ReservationID == reservationID && x.ClassID == ClassID && x.RoomLayoutID == RoomID);
                        r.ReservationStatus = 2;
                        db_context.Update(r);
                        db_context.SaveChanges();
                        Reservation waitlistEntry = db_context.Reservations.AsNoTracking().Where(x => x.ClassID == ClassID && x.RoomLayoutID == RoomID && x.ReservationStatus == 9).OrderBy(x => x.ReservationDate).FirstOrDefault();
                        List<Reservation> Reservations = db_context.Reservations.AsNoTracking().Where(x => x.ClassID == ClassID && x.RoomLayoutID == RoomID).ToList();

                        if(waitlistEntry!=null)
                        {
                            if(RoomID!=0)
                            {
                                RoomLayout roomLayout = db_context.RoomLayouts.AsNoTracking().SingleOrDefault(x => x.RoomLayoutID == RoomID);
                                dynamic layout= System.Json.JsonObject.Parse(roomLayout.SeatMatrix);
                                foreach(var entry in layout)
                                {
                                    if (entry.JsonType==System.Json.JsonType.Object) {
                                        if (entry["S"] == "C")
                                        {
                                            if (Reservations.Where(x => x.SeatNumber == int.Parse(entry["SN"])).Count() == 0)
                                            {
                                                waitlistEntry.SeatNumber = int.Parse(entry["SN"]);
                                                waitlistEntry.ReservationStatus = 1;
                                                db_context.Update(waitlistEntry);
                                                db_context.SaveChanges();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        result = new
                        {
                            StatusCode = "Success",
                            Message = "Successfully cancelled the reservation"
                        };
                        return Json(result);
                    }
                case "20"://Put Spot Under Maintenance
                    {
                        string SpotNumber= HttpContext.Request.Form["spotNumber"];
                        Reservation r = new Reservation();
                        r.UserID = 0;
                        r.RoomLayoutID = RoomID;
                        r.ClassID = ClassID;
                        r.ReservationDate = DateTime.UtcNow.ToTimeZoneTime(location.TimeZone);
                        r.SeatNumber = int.Parse(SpotNumber);
                        r.ReservationStatus = 3;
                        r.SignedIn = 0;
                        db_context.Add(r);
                        db_context.SaveChanges();
                        result = new
                        {
                            StatusCode = "Success",
                            Message = "Seat flagged as Under Maintenance"
                        };
                        return Json(result);
                    }
                default:
                    {
                        break;
                    }
            }
            return Json(true);
        }

        #region Members
        public PartialViewResult Members()
        {
            return PartialView();
        }
        
        [HttpPost]
        public JsonResult GetMemberList()
        {
            string para= HttpContext.Request.Form["q"][0];
            string[] queryPara = para.Split(' ');
            string para1 = "";
            string lastName = "";
           
            para1 = queryPara[0];
          
            if(queryPara.Count()==2)
            {
                lastName = queryPara[1];
            }
            List<User> Users = db_context.Users.AsNoTracking().Where(x => (x.FirstName.Contains(para1) || x.LastName.Contains(lastName) || x.Email.Contains(para1) || x.Email.Contains(lastName)) && x._UserRole==UserRoles.Member).ToList();
            return Json(Users);
        }

        [HttpPost]
        public JsonResult GetMembers()
        {
            var searchParam = HttpContext.Request.Form["search[value]"];
            var start = HttpContext.Request.Form["start"][0];
            var length = HttpContext.Request.Form["length"][0];
            var totalCount = db_context.Instructors.Count();
            var filterCount = db_context.Instructors.Count();
            List<dynamic> filteredMembers = db_context.Users.AsNoTracking().Where(x => x._UserRole <= UserRoles.Member).Skip(int.Parse(start)).Take(int.Parse(length)).Select(x => new { x.FirstName, x.LastName, x.Email, x.DateOfBirth,x.Phone }).ToList<dynamic>();

            string firstName, lastName;
            if (searchParam[0].Trim() != "")
            {
                string[] keywords = searchParam[0].Split(" ");
                if (keywords.Length > 1)
                {
                    firstName = keywords[0];
                    lastName = keywords[1];
                }
                else
                {
                    firstName = searchParam[0];
                    lastName = searchParam[0];
                }

                if (firstName != lastName)
                    filteredMembers = db_context.Users.AsNoTracking().Where(x => ((x.FirstName.Contains(firstName) && x.LastName.Contains(lastName)) || x.Email.Contains(searchParam)) && x._UserRole <= UserRoles.Member).Skip(int.Parse(start)).Take(int.Parse(length)).Select(x => new { x.FirstName, x.LastName, x.Email, x.DateOfBirth, x.Phone }).ToList<dynamic>();
                else
                    filteredMembers = db_context.Users.AsNoTracking().Where(x => (x.FirstName.Contains(firstName) || x.LastName.Contains(lastName) || x.Email.Contains(searchParam)) && x._UserRole <= UserRoles.Member).Skip(int.Parse(start)).Take(int.Parse(length)).Select(x => new { x.FirstName, x.LastName, x.Email, x.DateOfBirth, x.Phone }).ToList<dynamic>();

                filterCount = filteredMembers.Count();
            }
            //zfilteredInstructor.ForEach(x => x.Password = "");

            dynamic data = new
            {
                draw = HttpContext.Request.Form["draw"],
                recordsTotal = totalCount,
                recordsFiltered = filterCount,
                data = filteredMembers
            };
            return Json(data);
        }
        #endregion

        public PartialViewResult Configure()
        {
            return PartialView();
        }
        private bool IsAuthentic(string username, string password)
        {
            int count = db_context.Users.Count(x => x._UserRole >= UserRoles.Instructor && (x.Email == username||x.UserName==username) && x.Password == password);
            if (count == 1)
                return true;
            return false;
        }

        #region Configure
        #region Instructors
        public PartialViewResult Instructors()
        {
            return PartialView();
        }
        [HttpPost]
        public JsonResult GetInstructors()
        {
            User loggedInuser = db_context.Users.Where(x => x.Email == HttpContext.User.Identity.Name || x.UserName == HttpContext.User.Identity.Name).ToList().First();
            var searchParam = HttpContext.Request.Form["search[value]"];
            var start = HttpContext.Request.Form["start"][0];
            var length = HttpContext.Request.Form["length"][0];
            var totalCount = db_context.Instructors.Count();
            var filterCount = db_context.Instructors.Count();
            List<dynamic> filteredInstructor = db_context.Instructors.AsNoTracking().Where(x=>x._UserRole<=loggedInuser._UserRole).Skip(int.Parse(start)).Take(int.Parse(length)).Select(x => new { x.UserID, x.FirstName, x.LastName, x.Email, x.Bio }).ToList<dynamic>();

            string firstName, lastName;
            if (searchParam[0].Trim() != "")
            {
                string[] keywords = searchParam[0].Split(" ");
                if (keywords.Length > 1)
                {
                    firstName = keywords[0];
                    lastName = keywords[1];
                }
                else
                {
                    firstName = searchParam[0];
                    lastName = searchParam[0];
                }

                if (firstName != lastName)
                    filteredInstructor = db_context.Instructors.AsNoTracking().Where(x => ((x.FirstName.Contains(firstName) && x.LastName.Contains(lastName)) || x.Email.Contains(searchParam)) && x._UserRole<=loggedInuser._UserRole).Skip(int.Parse(start)).Take(int.Parse(length)).Select(x => new { x.UserID, x.FirstName, x.LastName, x.Email, x.Bio }).ToList<dynamic>();
                else
                    filteredInstructor = db_context.Instructors.AsNoTracking().Where(x => (x.FirstName.Contains(firstName) || x.LastName.Contains(lastName) || x.Email.Contains(searchParam)) && x._UserRole<=loggedInuser._UserRole).Skip(int.Parse(start)).Take(int.Parse(length)).Select(x => new { x.UserID, x.FirstName, x.LastName, x.Email, x.Bio }).ToList<dynamic>();

                filterCount = filteredInstructor.Count();
            }
            //zfilteredInstructor.ForEach(x => x.Password = "");

            dynamic data = new
            {
                draw = HttpContext.Request.Form["draw"],
                recordsTotal = totalCount,
                recordsFiltered = filterCount,
                data = filteredInstructor
            };
            return Json(data);


        }

        public PartialViewResult AddInstructor()
        {
            List<Location> Locations = db_context.Locations.AsNoTracking().ToList();
            ViewBag.Locations = Locations;
            return PartialView();
        }
        [HttpPost]
        public JsonResult AddInstructor(Instructor Instructor)
        {
            Instructor._UserRole = 49;
            Instructor.Password = "qwerty123";
            db_context.Instructors.Add(Instructor);
            db_context.SaveChanges();
            return Json(true);
        }

        public async Task<PartialViewResult> UpdateInstructor(int instructorID)
        {
            Instructor model = await db_context.Instructors.AsNoTracking().SingleAsync(x => x.UserID == instructorID);
            return PartialView(model);
        }

        [HttpPost]
        public JsonResult UpdateInstructor(Instructor Instructor)
        {
            Instructor model = db_context.Instructors.Single(x => x.UserID == Instructor.UserID);
            Instructor.Password = model.Password;
            Instructor._UserRole = model._UserRole;
            Instructor.ActivatedFlag = model.ActivatedFlag;
            db_context.Entry(model).CurrentValues.SetValues(Instructor);
            int result = db_context.SaveChanges();
            return Json(true);
        }

        #endregion

        #region Locations
        public PartialViewResult Locations()
        {
            return PartialView();
        }
        [HttpPost]
        public JsonResult GetLocations()
        {
            
            var searchParam = HttpContext.Request.Form["search[value]"];
            var start = HttpContext.Request.Form["start"][0];
            var length = HttpContext.Request.Form["length"][0];
            var totalCount = db_context.Locations.Count();
            var filterCount = db_context.Locations.Count();
            List<dynamic> filteredLocations = db_context.Locations.AsNoTracking().Skip(int.Parse(start)).Take(int.Parse(length)).ToList<dynamic>();

            if (searchParam[0].Trim() != "")
            {
                filteredLocations = db_context.Locations.AsNoTracking().Where(x=>x.LocationName== searchParam[0] || x.City== searchParam[0] || x.AddressLine1== searchParam[0] || x.AddressLine2== searchParam[0]).Skip(int.Parse(start)).Take(int.Parse(length)).ToList<dynamic>();
                filterCount = filteredLocations.Count();
            }

            dynamic data = new
            {
                draw = HttpContext.Request.Form["draw"],
                recordsTotal = totalCount,
                recordsFiltered = filterCount,
                data = filteredLocations
            };
            return Json(data);
        }

        public PartialViewResult AddLocation()
        {
            return PartialView();
        }
        [HttpPost]
        public JsonResult AddLocation(Location Location)
        {
            db_context.Locations.Add(Location);
            db_context.SaveChanges();
            return Json(true);
        }
        public async Task<PartialViewResult> UpdateLocation(int locationID)
        {
            Location model = await db_context.Locations.AsNoTracking().SingleAsync(x => x.LocationID == locationID);
            return PartialView(model);
        }

        [HttpPost]
        public JsonResult UpdateLocation(Location Location)
        {
            Location model = db_context.Locations.Single(x => x.LocationID == Location.LocationID);
            db_context.Entry(model).CurrentValues.SetValues(Location);
            int result = db_context.SaveChanges();
            return Json(true);
        }
        #endregion

        #region Class Packages
        public PartialViewResult ClassPackages()
        {
            return PartialView();
        }
        [HttpPost]
        public JsonResult GetClassPackages()
        {
            var searchParam = HttpContext.Request.Form["search[value]"];
            var start = HttpContext.Request.Form["start"][0];
            var length = HttpContext.Request.Form["length"][0];
            var totalCount = db_context.ClassPackages.Count();
            var filterCount = db_context.ClassPackages.Count();
            List<dynamic> filteredClassPackages = db_context.ClassPackages.AsNoTracking().Skip(int.Parse(start)).Take(int.Parse(length)).ToList<dynamic>();

            if (searchParam[0].Trim() != "")
            {
                filteredClassPackages = db_context.ClassPackages.AsNoTracking().Where(x => x.ClassPackageName.Contains(searchParam[0])||x.ClassPackageDescription.Contains(searchParam[0])||x.Badge.Contains(searchParam[0])).Skip(int.Parse(start)).Take(int.Parse(length)).ToList<dynamic>();
                filterCount = filteredClassPackages.Count();
            }

            dynamic data = new
            {
                draw = HttpContext.Request.Form["draw"],
                recordsTotal = totalCount,
                recordsFiltered = filterCount,
                data = filteredClassPackages
            };
            return Json(data);
        }
        public PartialViewResult AddClassPackage()
        {
            List<ClassType> classTypes = db_context.ClassTypes.AsNoTracking().ToList();
            ViewBag.ClassTypes = classTypes;
            return PartialView();
        }
        
        [HttpPost]
        public JsonResult AddClassPackage(ClassPackage ClassPackage)
        {
            db_context.ClassPackages.Add(ClassPackage);
            db_context.SaveChanges();
            return Json(true);
        }
        public async Task<PartialViewResult> UpdateClassPackage(int classPackageID)
        {
            List<ClassType> classTypes = db_context.ClassTypes.AsNoTracking().ToList();
            ViewBag.ClassTypes = classTypes;
            ClassPackage model = await db_context.ClassPackages.AsNoTracking().SingleAsync(x => x.ClassPackageID == classPackageID);
            return PartialView(model);
        }

        [HttpPost]
        public JsonResult UpdateClassPackage(ClassPackage ClassPackage)
        {
            ClassPackage model = db_context.ClassPackages.Single(x => x.ClassPackageID == ClassPackage.ClassPackageID);
            db_context.Entry(model).CurrentValues.SetValues(ClassPackage);
            int result = db_context.SaveChanges();
            return Json(true);
        }
        #endregion

        #region Class Types
        public PartialViewResult ClassTypes()
        {
            return PartialView();
        }

        [HttpPost]
        public JsonResult GetClassTypes()
        {
            var searchParam = HttpContext.Request.Form["search[value]"];
            var start = HttpContext.Request.Form["start"][0];
            var length = HttpContext.Request.Form["length"][0];
            var totalCount = db_context.ClassTypes.Count();
            var filterCount = db_context.ClassTypes.Count();
            List<dynamic> filteredClassTypes = db_context.ClassTypes.Include(c=>c.Location).Include(c=>c.Instructor).AsNoTracking().Skip(int.Parse(start)).Take(int.Parse(length)).ToList<dynamic>();

            if (searchParam[0].Trim() != "")
            {
                filteredClassTypes = db_context.ClassTypes.Include(c => c.Location).Include(c => c.Instructor).AsNoTracking().Where(x => x.ClassName.Contains(searchParam[0]) || x.ClassDescription.Contains(searchParam[0]) || x.Location.LocationName.Contains(searchParam[0]) || x.Instructor.FirstName.Contains(searchParam[0]) || x.Instructor.LastName.Contains(searchParam[0])).Skip(int.Parse(start)).Take(int.Parse(length)).ToList<dynamic>();
                filterCount = filteredClassTypes.Count();
            }

            dynamic data = new
            {
                draw = HttpContext.Request.Form["draw"],
                recordsTotal = totalCount,
                recordsFiltered = filterCount,
                data = filteredClassTypes
            };
            return Json(data);
        }

        public PartialViewResult AddClassType()
        {
            ViewBag.Locations= db_context.Locations.AsNoTracking().ToList<Location>();
            ViewBag.Instructors= db_context.Instructors.AsNoTracking().ToList<Instructor>();
            return PartialView();
        }

        [HttpPost]
        public JsonResult AddClassType(ClassType ClassType)
        {
            db_context.ClassTypes.Add(ClassType);
            db_context.SaveChanges();
            return Json(true);
        }

        public async Task<PartialViewResult> UpdateClassType(int ClassTypeID)
        {
            ClassType model = await db_context.ClassTypes.AsNoTracking().SingleAsync(x => x.ClassTypeID == ClassTypeID);
            ViewBag.Locations = db_context.Locations.AsNoTracking().ToList<Location>();
            ViewBag.Instructors = db_context.Instructors.AsNoTracking().ToList<Instructor>();
            return PartialView(model);
        }

        [HttpPost]
        public JsonResult UpdateClassType(ClassType ClassType)
        {
            ClassType model = db_context.ClassTypes.Single(x => x.ClassTypeID == ClassType.ClassTypeID);
            db_context.Entry(model).CurrentValues.SetValues(ClassType);
            int result = db_context.SaveChanges();
            return Json(true);
        }
        #endregion

        #region Room Layout
        public PartialViewResult RoomLayouts()
        {
            return PartialView();
        }

        [HttpPost]
        public JsonResult GetRoomLayouts()
        {
            var searchParam = HttpContext.Request.Form["search[value]"];
            var start = HttpContext.Request.Form["start"][0];
            var length = HttpContext.Request.Form["length"][0];
            var totalCount = db_context.RoomLayouts.Count();
            var filterCount = db_context.RoomLayouts.Count();
            List<dynamic> filteredRoomLayouts = db_context.RoomLayouts.Include(c => c.Location).AsNoTracking().Skip(int.Parse(start)).Take(int.Parse(length)).ToList<dynamic>();

            if (searchParam[0].Trim() != "")
            {
                filteredRoomLayouts = db_context.RoomLayouts.Include(c => c.Location).AsNoTracking().Where(x => x.RoomName.Contains(searchParam[0]) || x.Location.LocationName.Contains(searchParam[0])).Skip(int.Parse(start)).Take(int.Parse(length)).ToList<dynamic>();
                filterCount = filteredRoomLayouts.Count();
            }

            dynamic data = new
            {
                draw = HttpContext.Request.Form["draw"],
                recordsTotal = totalCount,
                recordsFiltered = filterCount,
                data = filteredRoomLayouts
            };
            return Json(data);
        }

        public JsonResult GetRoomLayoutsByLocation(int LocationID)
        {
            List<dynamic> filteredRoomLayouts=db_context.RoomLayouts.Include(c => c.Location).Where(c=>c.LocationID==LocationID).AsNoTracking().ToList<dynamic>();
            return Json(filteredRoomLayouts);
        }

        public PartialViewResult AddRoomLayout()
        {
            ViewBag.Locations = db_context.Locations.AsNoTracking().ToList<Location>();
            return PartialView();
        }

        [HttpPost]
        public JsonResult AddRoomLayout(RoomLayout RoomLayout)
        {
            db_context.RoomLayouts.Add(RoomLayout);
            db_context.SaveChanges();
            return Json(true);
        }

        public async Task<PartialViewResult> UpdateRoomLayout(int RoomLayoutID)
        {
            RoomLayout model = await db_context.RoomLayouts.AsNoTracking().SingleAsync(x => x.RoomLayoutID == RoomLayoutID);
            ViewBag.Locations = db_context.Locations.AsNoTracking().ToList<Location>();
            return PartialView(model);
        }

        [HttpPost]
        public JsonResult UpdateRoomLayout(RoomLayout RoomLayout)
        {
            RoomLayout model = db_context.RoomLayouts.Single(x => x.RoomLayoutID == RoomLayout.RoomLayoutID);
            db_context.Entry(model).CurrentValues.SetValues(RoomLayout);
            int result = db_context.SaveChanges();
            return Json(true);
        }
        #endregion

        #region Class Schedules
        public PartialViewResult ClassSchedules()
        {
            ViewBag.RoomLayouts = db_context.RoomLayouts.Include(x => x.Location).AsNoTracking().ToList();
            return PartialView();
        }
        [HttpPost]
        public JsonResult SaveClassScheduleChanges(Dictionary<long,long> inputData)
        {
            foreach(KeyValuePair<long,long> kv in inputData)
            {
                ClassSchedule cs = db_context.ClassSchedules.Include(x=>x.ClassType).ThenInclude(x=>x.Location).Single(x => x.ClassScheduleID == kv.Key);
                cs.RoomLayoutID = kv.Value;
                db_context.SaveChanges();
            }
            return Json(true);
        }

        [HttpPost]
        public JsonResult GetClassSchedules()
        {
            var searchParam = HttpContext.Request.Form["search[value]"];
            var start = HttpContext.Request.Form["start"][0];
            var length = HttpContext.Request.Form["length"][0];
            var totalCount = db_context.ClassSchedules.Count();
            var filterCount = db_context.ClassSchedules.Count();
            List<dynamic> filteredClassSchedules = db_context.ClassSchedules.Include(c => c.ClassType).ThenInclude(c=>c.Location).Include(c=>c.ClassType).ThenInclude(c=>c.Instructor).Include(r=>r.RoomLayout).AsNoTracking().Skip(int.Parse(start)).Take(int.Parse(length)).ToList<dynamic>();
            
            if (searchParam[0].Trim() != "")
            {
                filteredClassSchedules = db_context.ClassSchedules.Include(c => c.ClassType).ThenInclude(c=>c.Location).Include(c => c.ClassType).ThenInclude(c => c.Instructor).Include(r => r.RoomLayout).AsNoTracking().Where(x => x.ClassType.ClassName.Contains(searchParam[0]) || x.RoomLayout.RoomName.Contains(searchParam[0])).Skip(int.Parse(start)).Take(int.Parse(length)).ToList<dynamic>();
                filterCount = filteredClassSchedules.Count();
            }

            dynamic data = new
            {
                draw = HttpContext.Request.Form["draw"],
                recordsTotal = totalCount,
                recordsFiltered = filterCount,
                data = filteredClassSchedules
            };
            return Json(data);
        }

        public PartialViewResult AddClassSchedule()
        {
            ViewBag.RoomLayouts = db_context.RoomLayouts.Include(r=>r.Location).AsNoTracking().ToList<RoomLayout>();
            ViewBag.ClassTypes = db_context.ClassTypes.Include(c => c.Location).AsNoTracking().ToList<ClassType>();
            return PartialView();
        }

        [HttpPost]
        public JsonResult AddClassSchedule(ClassSchedule ClassSchedule)
        {
            db_context.ClassSchedules.Add(ClassSchedule);

            for (DateTime date = ClassSchedule.StartDate; date.Date <= ClassSchedule.EndDate.Date; date = date.AddDays(1))
            {
                if (ClassSchedule.Days.Contains(date.ToString("ddd")))
                {
                    Class c = new Class()
                    {
                        ClassScheduleID = ClassSchedule.ClassScheduleID,
                        ClassTypeID = ClassSchedule.ClassTypeID,
                        StartDate = date,
                        EndDate = date,
                        StartTime = ClassSchedule.StartTime,
                        EndTime = ClassSchedule.EndTime,
                        AllowWaitlist = true,
                        IsCancelled = false
                    };
                    db_context.Classes.Add(c);
                }
            }

            db_context.SaveChanges();
            return Json(true);
        }

        public async Task<PartialViewResult> UpdateClassSchedule(int ClassScheduleID)
        {
            ClassSchedule model = await db_context.ClassSchedules.AsNoTracking().SingleAsync(x => x.ClassScheduleID == ClassScheduleID);
            ViewBag.RoomLayouts = db_context.RoomLayouts.Include(r => r.Location).AsNoTracking().ToList<RoomLayout>();
            ViewBag.ClassTypes = db_context.ClassTypes.Include(c => c.Location).AsNoTracking().ToList<ClassType>();
            return PartialView(model);
        }

        [HttpPost]
        public JsonResult UpdateClassSchedule(ClassSchedule ClassSchedule)
        {
            ClassSchedule model = db_context.ClassSchedules.Single(x => x.ClassScheduleID == ClassSchedule.ClassScheduleID);
            db_context.Entry(model).CurrentValues.SetValues(ClassSchedule);
            int result = db_context.SaveChanges();
            return Json(true);
        }
        #endregion

        #endregion
    }
}