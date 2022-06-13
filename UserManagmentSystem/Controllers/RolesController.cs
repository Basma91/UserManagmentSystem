using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagmentSystem.ViewModels;

namespace UserManagmentSystem.Controllers
{
    [Authorize(Roles ="Admin")]
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;

        public RolesController(RoleManager<IdentityRole> roleManager)
        {
            this.roleManager = roleManager;
        }
        public async Task <IActionResult> Index()
        {
            return View(await roleManager.Roles.ToListAsync());
        }


        public async Task<IActionResult> Add(RoleFormViewModel model)
        {

            if (!ModelState.IsValid)
            {
                return View("Index", await roleManager.Roles.ToListAsync());
            }
            if (await roleManager.RoleExistsAsync(model.Name))
            {
                ModelState.AddModelError("Name", "Role is Exists");
                return RedirectToAction(nameof(Index));
            }

            await roleManager.CreateAsync(new IdentityRole() { Name = model.Name.Trim() });
            return RedirectToAction(nameof(Index));


        }
    }
}
