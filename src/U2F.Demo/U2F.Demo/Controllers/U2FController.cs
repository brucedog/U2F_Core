using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace U2F.Demo.Controllers
{
    public class U2FController : Controller
    {
        public U2FController()
        {
            
        }

        public IActionResult Index()
        {
            return View();
        }
        

        public IActionResult Error()
        {
            return View();
        }
    }
}
