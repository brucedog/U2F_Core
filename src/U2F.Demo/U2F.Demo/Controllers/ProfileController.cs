﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using U2F.Demo.DataStore;
using U2F.Demo.Models;
using U2F.Demo.Services;
using U2F.Core.Utils;
using U2F.Demo.ViewModel;

namespace U2F.Demo.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly U2FContext _dataContext;
        private readonly IMembershipService _membershipService;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(U2FContext dataContext, 
            IMembershipService membershipService,
            ILogger<ProfileController> logger)
        {
            _dataContext = dataContext;
            _membershipService = membershipService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                ModelState.AddModelError("", "User has timed out.");
                RedirectToAction("Login", "Home");
            }

            var user = await _dataContext.Users.FirstAsync(person => person.Name == HttpContext.User.Identity.Name);
            return View("Index", user);
        }

        public void AddDevice(string deviceResponse)
        {
            try
            {
                if (!HttpContext.User.Identity.IsAuthenticated)
                {
                    ModelState.AddModelError("", "User has timed out.");
                    RedirectToAction("Login", "Home");
                }
                _membershipService.CompleteRegistration(HttpContext.User.Identity.Name, deviceResponse);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message);
            }
        }

        // TODO need to validate
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

                return new JsonResult(Ok(JsonConvert.SerializeObject(registerModel)));
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message);
            }
            return NoContent();
        }

        // TODO need to validate
        public async Task<IActionResult> DeviceInfo(int deviceId)
        {
            try
            {
                if (!HttpContext.User.Identity.IsAuthenticated)
                {
                    ModelState.AddModelError("", "User has timed out.");
                    RedirectToAction("Login", "Home");
                }

                User user = await _dataContext.Users.FirstAsync(person => person.Name == HttpContext.User.Identity.Name);
                Device device = user.DeviceRegistrations.FirstOrDefault(f => f.Id == deviceId);
                dynamic formattedResult = new
                {
                    Id = device.Id,
                    KeyHandle = device.KeyHandle.ByteArrayToBase64String(),
                    PublicKey = device.PublicKey.ByteArrayToBase64String(),
                    Counter = device.Counter,
                    UpdatedOn = device.UpdatedOn
                };
                return Ok(formattedResult);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message);
            }
            return NoContent();
        }
    }
}