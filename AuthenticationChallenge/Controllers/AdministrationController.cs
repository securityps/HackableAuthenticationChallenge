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
    [Authorize(Roles = AuthorizationRoles.Administrator)]
    public class AdministrationController : Controller
    {
        private ApplicationDbContext _dbContext;
        UserManager<ApplicationUser> _userManager;
        SignInManager<ApplicationUser> _signInManager;
        RoleManager<IdentityRole> _roleManager;

        public AdministrationController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
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
            EditUserViewModel vm = new EditUserViewModel();
            foreach(IdentityRole role in _roleManager.Roles.ToList<IdentityRole>())
            {
                vm.AvailableRoles.Add(role.Name);
            }
            
            if (string.IsNullOrEmpty(id))
            {
                return View(vm);
            }

            ApplicationUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return RedirectToAction(nameof(AdministrationController.Index));

            vm.Username = user.UserName;
            vm.Email = user.Email;
            vm.AccountNumber = user.AccountNumber;
            vm.Balance = user.LastStatementBalance;
            vm.SSN = user.SocialSecurityNumber;
            vm.Roles = await _userManager.GetRolesAsync(user);
            vm.Id = id;

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditUserViewModel vm)
        {
            foreach (IdentityRole role in _roleManager.Roles.ToList<IdentityRole>())
            {
                vm.AvailableRoles.Add(role.Name);
            }

            if (vm==null)
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
                user = await _userManager.FindByIdAsync(vm.Id);
                IList<string> inRoles = await _userManager.GetRolesAsync(user);
                List<Task> roleTasks = new List<Task>();
                foreach(string role in inRoles)
                {
                    if(!vm.Roles.Contains(role) && AuthorizationRoles.AllRoles.Contains(role))
                    {
                        roleTasks.Add(_userManager.RemoveFromRoleAsync(user, role));
                    }
                }
                foreach(string role in vm.Roles)
                {
                    if (!inRoles.Contains(role))
                    {
                        roleTasks.Add(_userManager.AddToRoleAsync(user, role));
                    }
                }
                Task.WaitAll(roleTasks.ToArray());
                return RedirectToAction(nameof(this.Index));                
            }
            return View(vm);
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
        public IList<string> AvailableRoles { get; set; }
        public string SSN { get; set; }
        public string AccountNumber { get; set; }
        public string Balance { get; set; }
        public string Id { get; set; }

        public EditUserViewModel()
        {
            Roles = new List<string>();
            AvailableRoles = new List<string>();
        }
    }
}