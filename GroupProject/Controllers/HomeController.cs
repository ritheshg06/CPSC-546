using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GroupProject.Models;
using GroupProject.Constants;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace GroupProject.Controllers
{
    public class HomeController : Controller
    {
        private DB_Context db_context;
        public HomeController(DB_Context db_context)
        {
            this.db_context = db_context;
        }
        public IActionResult Index(string Error="",string RegisterMessage="")
        {
            ViewBag.Locations = db_context.Locations.AsNoTracking().ToList<Location>();
            ViewBag.LoginError = Error;
            ViewBag.RegisterMessage = RegisterMessage;
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
