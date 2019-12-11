using AuthenticationChallenge.Data;
using AuthenticationChallenge.Models;
using AuthenticationChallenge.Models.AccountViewModels;
using AuthenticationChallenge.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OtpNet;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AuthenticationChallenge.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
        }

        [TempData]
        public string ErrorMessage { get; set; }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null, string message = "")
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            ViewData["Message"] = AuthenticationChallengeConstants.GetMessage(message);
            ViewData["ReturnUrl"] = returnUrl;
            if (TempData.ContainsKey("ssn") && TempData.ContainsKey("account") && TempData.ContainsKey("balance"))
            {
                ViewData["ssn"] = TempData["ssn"];
                ViewData["account"] = TempData["account"];
                ViewData["balance"] = TempData["balance"];
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                ApplicationUser user = _userManager.Users.SingleOrDefault(u => u.UserName == model.Email);
                if (user != null)
                {
                    HttpContext.Session.SetString(AuthenticationChallengeConstants.SessionKeyUsername, user.UserName);
                    _logger.LogInformation("Username found");

                    if (user.HasSetupChallengeQuestions())
                    {
                        return await Task.FromResult(RedirectToAction(nameof(AnswerChallengeQuestions)));
                    }
                    else
                    {
                        return await Task.FromResult(RedirectToAction(nameof(EnterPassword)));
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }

            return await Task.FromResult(View(model));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> AlternateLogin(string message = "")
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            ViewData["Message"] = AuthenticationChallengeConstants.GetMessage(message);
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlternateLogin(AlternateLoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = _userManager.Users.SingleOrDefault(u => u.UserName == model.Email);
                if (user != null)
                {
                    string password = "";
                    string pin = "";
                    if (user.TotpEnabled)
                    {
                        password = model.Password.Substring(0, model.Password.Length - 6);
                        pin = model.Password.Substring(model.Password.Length - 6);

                        Totp totp = new Totp(user.TotpSecret);
                        if (!totp.VerifyTotp(pin, out long window, VerificationWindow.RfcSpecifiedNetworkDelay))
                        {
                            ModelState.AddModelError(string.Empty, "Login failed.");
                            model.Password = "";
                            return View(model);
                        }
                    }
                    else
                    {
                        password = model.Password;
                    }
                    Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(model.Email, password, false, false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction(nameof(ManageController.Index), nameof(ManageController).Replace("Controller", ""));
                    }
                }
            }

            ModelState.AddModelError(string.Empty, "Login failed.");
            model.Password = "";
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> AnswerChallengeQuestions()
        {
            string username = HttpContext.Session.GetString(AuthenticationChallengeConstants.SessionKeyUsername);
            if (username != null)
            {
                IQueryable<ChallengeQuestion> questions = _context.ChallengeQuestions.Take(2);
                SetupChallengeQuestionsViewModel model = new SetupChallengeQuestionsViewModel
                {
                    Username = username,
                    Question1 = questions.First(),
                    Question2 = questions.Skip(1).First()
                };

                return await Task.FromResult(View(model));
            }

            return await Task.FromResult(RedirectToAction(nameof(Login), new { Message = AuthenticationChallengeConstants.LoginPageMessageFailure }));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> AnswerChallengeQuestions(SetupChallengeQuestionsViewModel model)
        {
            if (ModelState.IsValid)
            {
                string username = HttpContext.Session.GetString(AuthenticationChallengeConstants.SessionKeyUsername);
                if (username != null)
                {
                    model.Username = username;
                    ApplicationUser user = _userManager.Users.SingleOrDefault(u => u.UserName == username);
                    if (user.VerifyAnswers(model.Answer1, model.Answer2))
                    {
                        HttpContext.Session.SetInt32(AuthenticationChallengeConstants.SessionKeyAnsweredChallengeQuestions, 1);
                        return await Task.FromResult(RedirectToAction(nameof(EnterPassword)));
                    }

                    ModelState.AddModelError(string.Empty, "Authentication failed");
                    return await Task.FromResult(View(model));
                }
            }

            return await Task.FromResult(RedirectToAction(nameof(Login), new { Message = AuthenticationChallengeConstants.LoginPageMessageFailure }));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> EnterPassword()
        {
            string username = HttpContext.Session.GetString(AuthenticationChallengeConstants.SessionKeyUsername);
            if (username != null)
            {
                EnterPasswordViewModel model = new EnterPasswordViewModel
                {
                    Username = username
                };
                ApplicationUser user = _userManager.Users.SingleOrDefault(u => u.UserName == username);
                int? answeredChallengeQuestions = HttpContext.Session.GetInt32(AuthenticationChallengeConstants.SessionKeyAnsweredChallengeQuestions);
                if (!user.HasSetupChallengeQuestions() || answeredChallengeQuestions == 1)
                {
                    return await Task.FromResult(View(model));
                }
            }

            return await Task.FromResult(RedirectToAction(nameof(Login), new { Message = AuthenticationChallengeConstants.LoginPageMessageFailure }));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> EnterPassword(EnterPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                string username = HttpContext.Session.GetString(AuthenticationChallengeConstants.SessionKeyUsername);
                if (username != null)
                {
                    int? answeredChallengeQuestions = HttpContext.Session.GetInt32(AuthenticationChallengeConstants.SessionKeyAnsweredChallengeQuestions);
                    ApplicationUser user = _userManager.Users.SingleOrDefault(u => u.UserName == username);
                    if (user.TotpEnabled)
                    {
                        ModelState.AddModelError(string.Empty, "Cannot log in here. User is set up for TOTP login.");
                        return View(model);
                    }
                    if (!user.HasSetupChallengeQuestions() || answeredChallengeQuestions == 1)
                    {
                        Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(username, model.Password, false, false);
                        if (result.Succeeded)
                        {
                            user = _userManager.Users.SingleOrDefault(u => u.UserName == username);
                            if (!user.HasSetupChallengeQuestions())
                            {
                                return RedirectToAction(nameof(SetupChallengeQuestions));
                            }
                            else
                            {
                                return RedirectToAction(nameof(ManageController.Index), nameof(ManageController).Replace("Controller", ""));
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                            model.Password = "";
                            model.Username = username;
                            return View(model);
                        }
                    }
                }
            }
            return RedirectToAction(nameof(Login), new { Message = AuthenticationChallengeConstants.LoginPageMessageFailure });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> SetupChallengeQuestions()
        {
            string username = HttpContext.Session.GetString(AuthenticationChallengeConstants.SessionKeyUsername);
            if (username != null)
            {
                IQueryable<ChallengeQuestion> questions = _context.ChallengeQuestions.Take(2);
                SetupChallengeQuestionsViewModel model = new SetupChallengeQuestionsViewModel
                {
                    Username = username,
                    Question1 = questions.First(),
                    Question2 = questions.Skip(1).First()
                };
                return await Task.FromResult(View(model));
            }

            return await Task.FromResult(RedirectToAction(nameof(Login), new { Message = AuthenticationChallengeConstants.LoginPageMessageFailure }));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SetupChallengeQuestions(SetupChallengeQuestionsViewModel model)
        {
            if (ModelState.IsValid)
            {
                string username = HttpContext.Session.GetString(AuthenticationChallengeConstants.SessionKeyUsername);
                if (username != null)
                {
                    model.Username = username;
                    ApplicationUser user = _userManager.Users.SingleOrDefault(u => u.UserName == username);
                    user.Question1 = _context.ChallengeQuestions.SingleOrDefault(q => q.ID == model.Question1.ID);
                    user.Question2 = _context.ChallengeQuestions.SingleOrDefault(q => q.ID == model.Question2.ID);
                    user.Answer1 = model.Answer1;
                    user.Answer2 = model.Answer2;
                    await _userManager.UpdateAsync(user);
                    return RedirectToAction(nameof(ManageController.Index), nameof(ManageController).Replace("Controller", ""));
                }
            }
            return RedirectToAction(nameof(Login), new { Message = AuthenticationChallengeConstants.LoginPageMessageFailure });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                Random random = new Random();
                string ssn = random.Next(0, 999999999).ToString("000000000");
                string account = random.Next(0, 999999999).ToString("000000000");
                string balance = random.Next(1, 999999999).ToString("0.00");
                ApplicationUser user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    SocialSecurityNumber = ssn,
                    AccountNumber = account,
                    LastStatementBalance = balance
                };

                IdentityResult result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    TempData["ssn"] = ssn;
                    TempData["account"] = account;
                    TempData["balance"] = balance;
                    _logger.LogInformation("User created a new account with password.");

                    return RedirectToAction(nameof(Login), new { Message = AuthenticationChallengeConstants.LoginPageMessageRegisterSuccess });
                }
                AddErrors(result);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            string username = HttpContext.Session.GetString(AuthenticationChallengeConstants.SessionKeyUsername);
            if (username != null)
            {
                ForgotPasswordViewModel model = new ForgotPasswordViewModel
                {
                    Email = username
                };
                return View(model);
            }
            return RedirectToAction(nameof(Login), new { Message = AuthenticationChallengeConstants.LoginPageMessageFailure });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                string username = HttpContext.Session.GetString(AuthenticationChallengeConstants.SessionKeyUsername);
                model.Email = username;
                if (username != null)
                {
                    ApplicationUser user = await _userManager.FindByNameAsync(username);
                    if (user.ValidateForgotPasswordFields(model.SocialSecurityNumber, model.AccountNumber, model.LastStatementBalance))
                    {
                        return RedirectToAction(nameof(ResetPassword));
                    }
                    ModelState.AddModelError(string.Empty, "Authentication failed.");
                }
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword()
        {
            string username = HttpContext.Session.GetString(AuthenticationChallengeConstants.SessionKeyUsername);
            if (username != null)
            {
                ApplicationUser user = await _userManager.FindByNameAsync(username);
                ResetPasswordViewModel model = new ResetPasswordViewModel
                {
                    Email = username
                };
                return View(model);
            }
            return RedirectToAction(nameof(Login), new { Message = AuthenticationChallengeConstants.LoginPageMessageFailure });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                string username = HttpContext.Session.GetString(AuthenticationChallengeConstants.SessionKeyUsername);
                if (username != null)
                {
                    ApplicationUser user = await _userManager.FindByNameAsync(username);
                    user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, model.Password);
                    await _userManager.UpdateAsync(user);
                    return RedirectToAction(nameof(Login), new { Message = AuthenticationChallengeConstants.LoginPageMessagePasswordReset });
                }
            }
            return View(model);
        }


        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        #endregion
    }
}
