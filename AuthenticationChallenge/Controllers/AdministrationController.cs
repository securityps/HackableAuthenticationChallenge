using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthenticationChallenge.Data;
using AuthenticationChallenge.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationChallenge.Controllers
{
    //[Authorize(Roles = AuthorizationRoles.Administrator)]
    public class AdministrationController : Controller
    {
        private ApplicationDbContext _dbContext;
        UserManager<ApplicationUser> _userManager;
        SignInManager<ApplicationUser> _signInManager;

        public AdministrationController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<ActionResult> Index()
        {
            List<ApplicationUser> users = _userManager.Users.ToList();
            List<IndexPageUserViewModel> vmList = new List<IndexPageUserViewModel>(users.Count);

            foreach (var user in users)
            {
                IndexPageUserViewModel vm = new IndexPageUserViewModel();
                vm.User = user;
                vm.Roles = await _userManager.GetRolesAsync(user);
                vmList.Add(vm);
            }
            return View(vmList);
        }
        [HttpGet]
        public async Task<ActionResult> Edit(string id)
        {
            if(string.IsNullOrEmpty(id))
            {
                return View(new EditUserViewModel());
            }

            ApplicationUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return RedirectToAction(nameof(AdministrationController.Index));
            EditUserViewModel vm = new EditUserViewModel()
            {
                Username = user.UserName,
                Email = user.Email,
                AccountNumber = user.AccountNumber,
                Balance = user.LastStatementBalance,
                SSN = user.SocialSecurityNumber,
                Roles = await _userManager.GetRolesAsync(user),
                Id = id
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditUserViewModel vm)
        {
            if(vm==null)
            {
                return RedirectToAction(nameof(this.Index));
            }            
            ApplicationUser user;
            if (vm.Id == null)
            {
                user = new ApplicationUser();
                user.UserName = vm.Username;
            }
            else
            {
                user = await _userManager.FindByIdAsync(vm.Id); 
            }
            
            if(user==null)
            {
                return RedirectToAction(nameof(this.Index));
            }

            user.Email = vm.Email;
            user.AccountNumber = vm.AccountNumber;
            user.SocialSecurityNumber = vm.SSN;
            user.LastStatementBalance = vm.Balance;

            IdentityResult result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(this.Index));
            }
            else
            {
                return View(vm);
            }
            
        }
    }

    public class IndexPageUserViewModel
    {
        public ApplicationUser User { get; set; }
        public IList<string> Roles;
    }    
    
    public class EditUserViewModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public IList<string> Roles { get; set; }
        public string SSN { get; set; }
        public string AccountNumber { get; set; }
        public string Balance { get; set; }
        public string Id { get; set; }

        public EditUserViewModel()
        {
            Roles = new List<string>();
        }
    }
}