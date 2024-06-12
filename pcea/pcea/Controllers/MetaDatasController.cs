using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using pcea.Models;

namespace pcea.Controllers
{
    public class MetaDatasController : Controller
    {
        private readonly PceaDbContext _DbContext;

        public MetaDatasController(PceaDbContext context)
        {
            _DbContext = context;
        }

        // GET: MetaDatas
        public async Task<IActionResult> Index()
        {
            return View(await _DbContext.MetaData.ToListAsync());
        }

        // GET: MetaDatas/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var metaData = await _DbContext.MetaData
                .FirstOrDefaultAsync(m => m.MetaDataType == id);
            if (metaData == null)
            {
                return NotFound();
            }

            return View(metaData);
        }

        // GET: MetaDatas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MetaDatas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MetaDataType")] MetaData metaData)
        {
            if (ModelState.IsValid)
            {
                _DbContext.Add(metaData);
                await _DbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(metaData);
        }

        // GET: MetaDatas/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var metaData = await _DbContext.MetaData.FindAsync(id);
            if (metaData == null)
            {
                return NotFound();
            }
            return View(metaData);
        }

        // POST: MetaDatas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MetaDataType")] MetaData metaData)
        {
            if (id != metaData.MetaDataType)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _DbContext.Update(metaData);
                    await _DbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MetaDataExists(metaData.MetaDataType))
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
            return View(metaData);
        }

        // GET: MetaDatas/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var metaData = await _DbContext.MetaData
                .FirstOrDefaultAsync(m => m.MetaDataType == id);
            if (metaData == null)
            {
                return NotFound();
            }

            return View(metaData);
        }

        // POST: MetaDatas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var metaData = await _DbContext.MetaData.FindAsync(id);
            _DbContext.MetaData.Remove(metaData);
            await _DbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MetaDataExists(string id)
        {
            return _DbContext.MetaData.Any(e => e.MetaDataType == id);
        }


        /// <summary>
        /// Returns a list of all pre-defined field references for the specified METADATATYPE
        /// </summary>
        /// <param name="MetaDataType"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetReferences(string MetaDataType)
        {
            ViewBag.MetaDataType = MetaDataType;
            return PartialView("RefIndex", await _DbContext.MetaDataRef.Where(m => m.MetaDataType == MetaDataType).ToListAsync());

        }


        [HttpGet]
        public IActionResult Delete(int id)
        {
            try
            {
                //check if form already got entries

                _DbContext.MetaDataRef.Remove(_DbContext.MetaDataRef.FirstOrDefault(m => m.RecId == id));
                _DbContext.SaveChanges();

                TempData["message"] = "Form deleted successfully";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        //GET Edit Reference Popup modal
        public async Task<IActionResult> ShowModalEdit(string RecId)
        {
            return PartialView("RefEdit", await _DbContext.MetaDataRef.FindAsync(long.Parse(RecId)));
        }

        //GET Addnew Popup modal
        public IActionResult ShowModalRefNew(string metaType)
        {
            ViewBag.MetaDataType = metaType;
            var _metaref = new MetaDataRef();
            _metaref.MetaDataType = metaType;
            return PartialView("RefNew", _metaref);
        }

        //[HttpPost]
        public async Task<IActionResult> SaveNewRef(string sMetaType, string sRefId)
        {
            try
            {
                var metarefnew = await _DbContext.MetaDataRef.FirstOrDefaultAsync(m => m.MetaDataType == sMetaType && m.ReferenceId == sRefId);
                if (metarefnew == null)
                {
                    var _metaref = new MetaDataRef();
                    _metaref.MetaDataType = sMetaType;
                    _metaref.ReferenceId = sRefId;
                    _DbContext.Add(_metaref);
                    await _DbContext.SaveChangesAsync();
                    return Ok();
                }
                else
                {
                    metarefnew.ReferenceId = sRefId;
                    _DbContext.Update(metarefnew);
                    await _DbContext.SaveChangesAsync();
                    return Ok();
                }

            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest();
            }

        }

        public async Task<IActionResult> EditRef(string sMetaType, string sRefId, string sRecId)
        {
            try
            {
                var metarefnew = await _DbContext.MetaDataRef.FirstOrDefaultAsync(m => m.MetaDataType == sMetaType && m.RecId == long.Parse(sRecId));
                if (metarefnew == null)
                {
                    return NotFound();
                }
                metarefnew.ReferenceId = sRefId;
                _DbContext.Update(metarefnew);
                await _DbContext.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest();
            }

        }
    }
}
