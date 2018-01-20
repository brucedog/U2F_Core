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
using U2F.Demo.ViewModel;

namespace U2F.Demo.Controllers
{
    public class U2FController : Controller
    {
        private readonly ILogger<U2FController> _logger;
        private readonly IMembershipService _membershipService;

        public U2FController(IMembershipService membershipService, ILogger<U2FController> logger)
        {
            _membershipService = membershipService;
            _logger = logger;
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

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> BeginLogin(StartLoginViewModel model)
        {
            bool isUserRegistered = await _membershipService.IsUserRegistered(model.UserName);
            bool areCredsValid = await _membershipService.IsValidUserNameAndPassword(model.UserName, model.Password);

            if (string.IsNullOrWhiteSpace(model.Password) || !isUserRegistered)
            {
                _logger.LogInformation($"invalid username {model.UserName} or password {model.Password}");
                // If we got this far, something failed, redisplay form
                ModelState.AddModelError("CustomError", "User has not been registered.");
                return View("Login", model);
            }

            if (!areCredsValid)
            {
                _logger.LogInformation($"invalid username {model.UserName} or password {model.Password}");
                ModelState.AddModelError("CustomError", "User/Password is not invalid.");
                return View("Login", model);
            }

            try
            {
                ServerRegisterResponse baseServerChallenge = await _membershipService.GenerateServerChallenge(model.UserName);
                List<ServerChallenge> serverChallenge = await _membershipService.GenerateServerChallenges(model.UserName);

                if (serverChallenge == null || serverChallenge.Count == 0)
                    throw new Exception("No server challenges were generated.");

                var challenges = JsonConvert.SerializeObject(serverChallenge);
                CompleteLoginViewModel loginModel = new CompleteLoginViewModel
                {
                    AppId = baseServerChallenge.AppId,
                    Version = baseServerChallenge.Version,
                    Challenge = baseServerChallenge.Challenge,
                    Challenges = challenges,
                    UserName = model.UserName.Trim()
                };
                return View("FinishLogin", loginModel);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                ModelState.AddModelError("CustomError", e.Message);
                return View("Login", model);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CompletedLogin(CompleteLoginViewModel model)
        {
            bool isUserRegistered = await _membershipService.IsUserRegistered(model.UserName);
            if (!isUserRegistered)
            {
                // If we got this far, something failed, redisplay form
                ModelState.AddModelError("", "User has not been registered.");
                return View("FinishLogin", model);
            }

            try
            {
                if (!await _membershipService.AuthenticateUser(model.UserName, model.DeviceResponse))
                    throw new Exception("Device response did not work with user.");
                
                return RedirectToAction("Index", "Profile");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);

                ModelState.AddModelError("", "Error authenticating");
                return View("FinishLogin", model);
            }
        }

        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> BeginRegister(StartRegisterViewModel viewModel)
        {
            bool isUserRegistered = await _membershipService.IsUserRegistered(viewModel.UserName);
            if (isUserRegistered)
            {
                ModelState.AddModelError("CustomError", "User is already registered.");
                return View("Register", viewModel);
            }

            if (!string.IsNullOrWhiteSpace(viewModel.Password)
                && !string.IsNullOrWhiteSpace(viewModel.UserName)
                && viewModel.Password.Equals(viewModel.ConfirmPassword))
            {
                try
                {
                    bool result = await _membershipService.SaveNewUser(viewModel.UserName, viewModel.Password, viewModel.Email);
                    if (!result)
                        throw new Exception("Failed to create user");

                    ServerRegisterResponse serverRegisterResponse = await _membershipService.GenerateServerChallenge(viewModel.UserName);

                    CompleteRegisterViewModel registerModel = new CompleteRegisterViewModel
                    {
                        UserName = viewModel.UserName,
                        AppId = serverRegisterResponse.AppId,
                        Challenge = serverRegisterResponse.Challenge,
                        Version = serverRegisterResponse.Version
                    };

                    return View("FinishRegister", registerModel);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                    ModelState.AddModelError("CustomError", e.Message);

                    return View("Register", viewModel);
                }
            }

            ModelState.AddModelError("CustomError", "invalid input");
            return View("Register", viewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CompleteRegister(CompleteRegisterViewModel value)
        {
            if (!string.IsNullOrWhiteSpace(value.DeviceResponse)
                && !string.IsNullOrWhiteSpace(value.UserName))
            {
                try
                {
                    value.DeviceResponse = await _membershipService.CompleteRegistration(value.UserName, value.DeviceResponse)
                        ? "Registration was successful."
                        : "Registration failed.";

                    return View("SucessfulRegister", new CompleteRegisterViewModel { UserName = value.UserName, DeviceResponse = value.DeviceResponse });
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                    ModelState.AddModelError("CustomError", e.Message);

                    return View("FinishRegister", value);
                }
            }

            ModelState.AddModelError("CustomError", "bad username/device response");
            return View("FinishRegister", value);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await _membershipService.SignOut();
            _logger.LogInformation(4, "User logged out.");
            return RedirectToAction("Index", "U2F");
        }
    }
}