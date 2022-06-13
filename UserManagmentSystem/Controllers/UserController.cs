using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagmentSystem.Models;
using UserManagmentSystem.ViewModels;

namespace UserManagmentSystem.Controllers
{
    [Authorize(Roles ="Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> usermanger;
        private readonly RoleManager<IdentityRole> rolemanager;

        public UserController(UserManager<ApplicationUser> user,RoleManager<IdentityRole> role)
        {
            this.usermanger = user;
            this.rolemanager = role;
        }
        public async Task<IActionResult> Index()
        {
            var users = await usermanger.Users.Select(u => new UserViewModel()
            {
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Id = u.Id,
                UserName = u.UserName,
                Roles = usermanger.GetRolesAsync(u).Result
            }).ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> Add()
        {
            var roles = await rolemanager.Roles.Select(m=>new RolesViewModel() {RoleId=m.Id,RoleName=m.Name }).ToListAsync();
            var User = new NewUserViewModel()
            {
                Roles=roles
            };
            return View(User);
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(NewUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!model.Roles.Any(m => m.IsSelected) || model.Roles==null)
            {
                ModelState.AddModelError("Roles", "Please Select a role");
                return View(model);
            }
            if (await usermanger.FindByEmailAsync(model.Email) !=null)
            {
                ModelState.AddModelError("Email", "This Email is Exists");
                return View(model);
            }

            if (await usermanger.FindByNameAsync(model.UserName) != null)
            {
                ModelState.AddModelError("UserName", "This UserName is Exists");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName =model.UserName,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };
            var result = await usermanger.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            await usermanger.AddToRolesAsync(user, model.Roles.Where(r=>r.IsSelected).Select(m => m.RoleName));
            return RedirectToAction(nameof(Index));
            }
        public async Task<IActionResult> ManageRoles(string userid)
        {
            var user = await usermanger.FindByIdAsync(userid);
            if (user == null)
                return NotFound();

            var roles = await rolemanager.Roles.ToListAsync();

            var userRoles = new UserRolesViewModel()
            {
                UserId = user.Id,
                UserName = user.UserName,
                Roles = roles.Select(r => new RolesViewModel()
                {
                    RoleId = r.Id,
                    RoleName = r.Name,
                    IsSelected = usermanger.IsInRoleAsync(user, r.Name).Result
                }).ToList()
            };
            return View( userRoles);
          // var Roles=await user.GetRolesAsync(user).Result
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageRoles(UserRolesViewModel model)
        {
            var user = usermanger.FindByIdAsync(model.UserId);
            if (user == null)
                return NotFound();

            var userRoles =await usermanger.GetRolesAsync(user.Result);
            foreach(var role in model.Roles)
            {
                if (role.IsSelected &&!userRoles.Any(m=>m==role.RoleName))
                {
                    await usermanger.AddToRoleAsync(user.Result, role.RoleName);
                }

                if (!role.IsSelected && userRoles.Any(m => m == role.RoleName))
                {
                    await usermanger.RemoveFromRoleAsync(user.Result, role.RoleName);
                }
            }
            return RedirectToAction(nameof(Index));

        }
    }
}
