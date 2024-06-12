using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using pcea.Models;
using pceaLibrary;

namespace pcea.Controllers
{
    public class AppRolesController : Controller
    {
        public readonly AppRole appRole;
        private readonly PceaDbContext _DbContext;

        public AppRolesController(PceaDbContext context)
        {
            _DbContext = context;
        }

        // GET: AppRoles
        public async Task<IActionResult> Index()
        {
            //AppRole appRole = new AppRole();
            //ViewBag.Rolename = appRole.RoleName;

            return View(await _DbContext.AppRole.ToListAsync());
        }

        // GET: AppRoles/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appRole = await _DbContext.AppRole
                .FirstOrDefaultAsync(m => m.RoleId == id);
            if (appRole == null)
            {
                return NotFound();
            }

            return View(appRole);
        }

        // GET: AppRoles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AppRoles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoleId,RoleName")] AppRole appRole)
        {
            if (ModelState.IsValid)
            {
                _DbContext.Add(appRole);
                await _DbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(appRole);
        }

        // GET: AppRoles/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appRole = await _DbContext.AppRole.FindAsync(id);
            if (appRole == null)
            {
                return NotFound();
            }
            return View(appRole);
        }

        // POST: AppRoles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("RoleId,RoleName")] AppRole appRole)
        {
            if (id != appRole.RoleId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _DbContext.Update(appRole);
                    await _DbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppRoleExists(appRole.RoleId))
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
            return View(appRole);
        }

        // GET: AppRoles/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appRole = await _DbContext.AppRole
                .FirstOrDefaultAsync(m => m.RoleId == id);
            if (appRole == null)
            {
                return NotFound();
            }

            return View(appRole);
        }

        // POST: AppRoles/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var appRole = await _DbContext.AppRole.FindAsync(id);
            _DbContext.AppRole.Remove(appRole);
            await _DbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AppRoleExists(string id)
        {
            return _DbContext.AppRole.Any(e => e.RoleId == id);
        }

        //Gets The Role details view
        public string GetPriviledges(string sRoleId)
        {
            UserMgt _usermgt = new UserMgt();

            var privs = JsonConvert.DeserializeObject<List<AppPrivilege>>(_usermgt.GetUserAssignedPrivileges(sRoleId));
            var appPrivs = _DbContext.AppPrivilege.Where(w => !privs.Contains(w)).ToList();

            appPrivs.AddRange(privs);


            return JsonConvert.SerializeObject(appPrivs);
        }

        [HttpGet]
        public string SavePrivileges(string sRoleId, string sPrivileges)
        {
            if (sRoleId.Trim() == string.Empty)
            {
                return "Invalid Role was selected Operation aborted.";
            }
            var roledetails = _DbContext.UserPrivilege.Where(r => r.RoleId == sRoleId).ToList();

            if (string.IsNullOrEmpty(sPrivileges))
            {
                //var details = _DbContext.UserPrivilege.Where(e => e.RoleId == sRoleId).ToList();
                _DbContext.UserPrivilege.RemoveRange(roledetails);
                _DbContext.SaveChanges(true);
                return "Privileges have been reset.";
            }

            UserMgt _usermgt = new UserMgt();
            //int iTotalRec = _usermgt.SaveUserAssignedPrivileges(sRoleId, sPrivileges);
            int iTotalRec = roledetails.Count;
            //if (iTotalRec == 0) 
            //{
            //if (roledetails != null)
            _DbContext.UserPrivilege.RemoveRange(roledetails);

            string[] privileges = sPrivileges.Split("-");
                var userDetails = _DbContext.AppPrivilege.Where(e => privileges.ToList().Contains(e.PrivilegeId.ToString())).ToList();
                //int i = 0;
                foreach (AppPrivilege item in userDetails)
                {
                    UserPrivilege userPrivilege = new UserPrivilege()
                    {
                        Assigned = true,
                        PrivilegeId = item.PrivilegeId,
                        RoleId = sRoleId
                    };
                    _DbContext.UserPrivilege.Add(userPrivilege);
                    //i++;
                    
                }
            var check = _DbContext.SaveChanges(true);
            iTotalRec = check - roledetails.Count;
            //if(userDetails.Count != iTotalRec)
            //    return _usermgt.FriendlyErrorMessage;
            //}
            //else
            //{
            //    return iTotalRec.ToString() + " privileges assigned.";
            //}

            return iTotalRec.ToString() + " privileges assigned.";

        }


        /* public ActionResult ShowModalNewRole()
         {
             return PartialView("NewRole");
         }*/

        //Save a new app role
        public async Task<IActionResult> SaveNewRole(string sRoleName)
        {
            try
            {
                sRoleName = sRoleName.ToUpper();
                var newRole = await _DbContext.AppRole.FirstOrDefaultAsync(e => e.RoleName == sRoleName);
                if (newRole != null)
                {
                    ViewBag.Message = "Role already exists...Saving failed!";
                    return Ok();
                }
                var _appRole = new AppRole();
                _appRole.RoleName = sRoleName.Replace("_", " ");
                _appRole.RoleId = sRoleName;
                _DbContext.Add(_appRole);
                await _DbContext.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return View("Index");
            }

        }
    }
}
