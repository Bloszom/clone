using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using pcea.Models;

namespace pcea.Controllers
{
    public class WorkflowManagersController : Controller
    {
        private readonly PceaDbContext _context;

        public WorkflowManagersController(PceaDbContext context)
        {
            _context = context;
        }

        // GET: WorkflowManagers
        public async Task<IActionResult> Index()
        {
            return View(await _context.WorkflowManager.ToListAsync());
        }

        // GET: WorkflowManagers/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workflowManager = await _context.WorkflowManager
                .FirstOrDefaultAsync(m => m.RecId == id);
            if (workflowManager == null)
            {
                return NotFound();
            }

            return View(workflowManager);
        }

        // GET: WorkflowManagers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: WorkflowManagers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RecId,TaskId,TaskType,ProcessId,ReferenceNo,OperatorName,DateAssigned,ActionId,ActionUrl,UserId,RoleId,Remarks,DateCompleted,CompletionFlag")] WorkflowManager workflowManager)
        {
            if (ModelState.IsValid)
            {
                _context.Add(workflowManager);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(workflowManager);
        }

        // GET: WorkflowManagers/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workflowManager = await _context.WorkflowManager.FindAsync(id);
            if (workflowManager == null)
            {
                return NotFound();
            }
            return View(workflowManager);
        }

        // POST: WorkflowManagers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("RecId,TaskId,TaskType,ProcessId,ReferenceNo,OperatorName,DateAssigned,ActionId,ActionUrl,UserId,RoleId,Remarks,DateCompleted,CompletionFlag")] WorkflowManager workflowManager)
        {
            if (id != workflowManager.RecId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(workflowManager);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WorkflowManagerExists(workflowManager.RecId))
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
            return View(workflowManager);
        }

        // GET: WorkflowManagers/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workflowManager = await _context.WorkflowManager
                .FirstOrDefaultAsync(m => m.RecId == id);
            if (workflowManager == null)
            {
                return NotFound();
            }

            return View(workflowManager);
        }

        // POST: WorkflowManagers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var workflowManager = await _context.WorkflowManager.FindAsync(id);
            _context.WorkflowManager.Remove(workflowManager);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WorkflowManagerExists(long id)
        {
            return _context.WorkflowManager.Any(e => e.RecId == id);
        }
    }
}
