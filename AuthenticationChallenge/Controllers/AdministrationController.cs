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
    }

    public class IndexPageUserViewModel
    {
        public ApplicationUser User { get; set; }
        public IList<string> Roles;
    }
    
}