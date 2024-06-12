using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using pcea.Models;
using pceaLibrary;

namespace pcea.Controllers
{
    public class UserProfilesController : Controller
    {
        private readonly PceaDbContext _DbContext;

        public UserProfilesController(PceaDbContext context)
        {
            _DbContext = context;
        }

        // GET: UserProfiles
        public async Task<IActionResult> Index()
        {
            ViewBag.RoleList = _DbContext.AppRole.Select(a => new SelectListItem
            {
                Value = a.RoleId,
                Text = a.RoleName
            }).ToList();
            return View(await _DbContext.AppUserProfileView.Distinct().ToListAsync());
        }

        //Get User Status Popup
        public async Task<IActionResult> GetUserStatus(string UserId)
        {
            var userProfile = await _DbContext.UserProfile.FindAsync(UserId);
            if (userProfile == null)
            {
                return NotFound();
            }
            userProfile.StatusList = Vars.UserStatus;
            return PartialView("ChangeStatus", userProfile);
        }

        //Save User Status
        [HttpPost]
        public async Task<IActionResult> SaveUserStatus(string sUserId, string sStatus)
        {
            try
            {
                var userProfile = await _DbContext.UserProfile.FirstOrDefaultAsync(m => m.UserId == sUserId);
                if (userProfile == null)
                {
                    return NotFound();
                }
                userProfile.Status = sStatus;
                _DbContext.Update(userProfile);
                await _DbContext.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserProfileExists(sUserId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }        
        
        
        [HttpPost]
        public async Task<IActionResult> SaveUserRole(string sUser, string sRole)
        {
            try
            {
                var userProfile = await _DbContext.UserProfile.FirstOrDefaultAsync(m => m.UserId == sUser);
                if (userProfile == null)
                {
                    return NotFound();
                }
                userProfile.RoleId = sRole;
                _DbContext.Update(userProfile);
                await _DbContext.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserProfileExists(sRole))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        //Get User Role Popup
        public async Task<IActionResult> GetUserRole(string UserId)
        {
            var userProfile = await _DbContext.UserProfile.FindAsync(UserId);
            if (userProfile == null)
            {
                return NotFound();
            }
            userProfile.RoleList = _DbContext.AppRole.Select(a => new SelectListItem
            {
                Value = a.RoleId,
                Text = a.RoleName
            }).ToList();
            return PartialView("ChangeRole", userProfile);
        }
/*        // GET: UserProfiles/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userProfile = await _DbContext.UserProfile
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (userProfile == null)
            {
                return NotFound();
            }

            return View(userProfile);
        }

        // GET: UserProfiles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: UserProfiles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,Fullname,JobTitle,Telephone,Email,RoleId,ImageUrl,Password,Status")] UserProfile userProfile)
        {
            if (ModelState.IsValid)
            {
                _DbContext.Add(userProfile);
                await _DbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(userProfile);
        }

        // GET: UserProfiles/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userProfile = await _DbContext.UserProfile.FindAsync(id);
            if (userProfile == null)
            {
                return NotFound();
            }
            return View(userProfile);
        }

        // POST: UserProfiles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("UserId,Fullname,JobTitle,Telephone,Email,RoleId,ImageUrl,Password,Status")] UserProfile userProfile)
        {
            if (id != userProfile.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _DbContext.Update(userProfile);
                    await _DbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserProfileExists(userProfile.UserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(userProfile);
        }

        // GET: UserProfiles/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userProfile = await _DbContext.UserProfile
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (userProfile == null)
            {
                return NotFound();
            }

            return View(userProfile);
        }

        // POST: UserProfiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var userProfile = await _DbContext.UserProfile.FindAsync(id);
            _DbContext.UserProfile.Remove(userProfile);
            await _DbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }*/

        private bool UserProfileExists(string id)
        {
            return _DbContext.UserProfile.Any(e => e.UserId == id);
        }
    }
}
