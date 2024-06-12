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
    public class MetaDataRefsController : Controller
    {
        private readonly PceaDbContext _context;

        public MetaDataRefsController(PceaDbContext context)
        {
            _context = context;
        }

        // GET: MetaDataRefs
        public async Task<IActionResult> Index()
        {
            return View(await _context.MetaDataRef.ToListAsync());
        }

        // GET: MetaDataRefs/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var metaDataRef = await _context.MetaDataRef
                .FirstOrDefaultAsync(m => m.RecId == id);
            if (metaDataRef == null)
            {
                return NotFound();
            }

            return View(metaDataRef);
        }

        // GET: MetaDataRefs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MetaDataRefs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RecId,ReferenceId,MetaDataType")] MetaDataRef metaDataRef)
        {
            if (ModelState.IsValid)
            {
                _context.Add(metaDataRef);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(metaDataRef);
        }

        // GET: MetaDataRefs/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var metaDataRef = await _context.MetaDataRef.FindAsync(id);
            if (metaDataRef == null)
            {
                return NotFound();
            }
            return View(metaDataRef);
        }

        // POST: MetaDataRefs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("RecId,ReferenceId,MetaDataType")] MetaDataRef metaDataRef)
        {
            if (id != metaDataRef.RecId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(metaDataRef);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MetaDataRefExists(metaDataRef.RecId))
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
            return View(metaDataRef);
        }

        // GET: MetaDataRefs/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var metaDataRef = await _context.MetaDataRef
                .FirstOrDefaultAsync(m => m.RecId == id);
            if (metaDataRef == null)
            {
                return NotFound();
            }

            return View(metaDataRef);
        }

        // POST: MetaDataRefs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var metaDataRef = await _context.MetaDataRef.FindAsync(id);
            _context.MetaDataRef.Remove(metaDataRef);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MetaDataRefExists(long id)
        {
            return _context.MetaDataRef.Any(e => e.RecId == id);
        }
    }
}
