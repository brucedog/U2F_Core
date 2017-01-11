using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using U2F.Demo.Models;
using U2F.Demo.Services;
using U2F.Core.Utils;
using U2F.Demo.ViewModel;

namespace U2F.Demo.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IMembershipService _membershipService;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(IMembershipService membershipService,
            ILogger<ProfileController> logger)
        {
            _membershipService = membershipService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                ModelState.AddModelError("", "User has timed out.");
                RedirectToAction("Login", "U2F");
            }

            var user = await _membershipService.FindUserByUsername(HttpContext.User.Identity.Name);
            return View("Index", user);
        }

        public async Task<IActionResult> AddDevice(string deviceResponse)
        {
            try
            {
                if (!HttpContext.User.Identity.IsAuthenticated)
                {
                    ModelState.AddModelError("", "User has timed out.");
                    RedirectToAction("Login", "U2F");
                }
                bool result = await _membershipService.CompleteRegistration(HttpContext.User.Identity.Name, deviceResponse);
                if(result)
                    return Ok();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message);
                return StatusCode(500);
            }
            return BadRequest();
        }
        
        public async Task<IActionResult> GetChallenge()
        {
            try
            {
                List<ServerChallenge> serverRegisterResponse = await _membershipService.GenerateServerChallenges(HttpContext.User.Identity.Name);
                CompleteRegisterViewModel registerModel = new CompleteRegisterViewModel
                {
                    UserName = HttpContext.User.Identity.Name,
                    AppId = serverRegisterResponse[0].appId,
                    Challenge = serverRegisterResponse[0].challenge,
                    Version = serverRegisterResponse[0].version
                };

                return new JsonResult(JsonConvert.SerializeObject(registerModel));
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message);
            }
            return NoContent();
        }
        
        public async Task<IActionResult> DeviceInfo(int deviceId)
        {
            try
            {
                if (!HttpContext.User.Identity.IsAuthenticated)
                {
                    ModelState.AddModelError("", "User has timed out.");
                    RedirectToAction("Login", "U2F");
                }

                User user = await _membershipService.FindUserByUsername(HttpContext.User.Identity.Name);
                Device device = user.DeviceRegistrations.FirstOrDefault(f => f.Id == deviceId);
                dynamic formattedResult = new
                {
                    Id = device.Id,
                    KeyHandle = device.KeyHandle.ByteArrayToBase64String(),
                    PublicKey = device.PublicKey.ByteArrayToBase64String(),
                    Counter = device.Counter,
                    UpdatedOn = device.UpdatedOn
                };
                return new JsonResult(JsonConvert.SerializeObject(formattedResult));
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message);
            }
            return NoContent();
        }
    }
}