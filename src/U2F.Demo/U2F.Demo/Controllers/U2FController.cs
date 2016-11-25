using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using U2F.Demo.Services;

namespace U2F.Demo.Controllers
{
    public class U2FController : Controller
    {
        private IMembershipService MembershipServicem { get; set; }

        public U2FController(IMembershipService membershipServicem)
        {
            MembershipServicem = membershipServicem;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }
        

        public IActionResult Error()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }
    }
}
