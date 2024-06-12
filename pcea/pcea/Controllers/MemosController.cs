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
    public class MemosController : Controller
    {
        private readonly PceaDbContext _DbContext;

        public MemosController(PceaDbContext context)
        {
            _DbContext = context;
        }

        // GET: Memos
        public async Task<IActionResult> Index()
        {
            return View(await _DbContext.Memo.ToListAsync());
        }

        // GET: Memos/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var memo = await _DbContext.Memo
                .FirstOrDefaultAsync(m => m.RecId == id);
            if (memo == null)
            {
                return NotFound();
            }

            return View(memo);
        }

        // GET: Memos/Create
        public IActionResult Build(long? id)
        {
            var model = new Memo();
            if (id == null)
            {
                model.ProcessList = _DbContext.Workflow.Select(a => new SelectListItem
                {
                    Value = a.ProcessId.ToString(),
                    Text = a.ProcessName
                }).ToList();
                model.DateCreated = DateTime.Now;
                model.Published = false;
                ////MemoTypeList = _DbContext.MetaDataRef.Where(a => a.MetaDataType == Vars.MetaDataTypes["MEMO"]).Select(a => new SelectListItem
                ////{
                ////    Value = a.ReferenceId.ToString(),
                ////    Text = a.ReferenceId
                ////}).ToList(),

                //TaskId = Vars.TaskTypes["UNASSIGNED"]
            }
            else
            {
                var memo = _DbContext.Memo.SingleOrDefault(x => x.RecId == id);

                model.ProcessList = _DbContext.Workflow.Select(a => new SelectListItem
                {
                    Value = a.ProcessId.ToString(),
                    Text = a.ProcessName
                }).ToList();
                model.DateCreated = memo.DateCreated;
                model.Published = memo.Published;
                model.MemoContent = memo.MemoContent;
                model.MemoName = memo.MemoName;
                model.RecId = memo.RecId;
                model.ProcessId = memo.ProcessId;
            }


            return View(model);
        }

        // POST: Memos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Build([Bind("RecId,MemoName,MemoContent,DateCreated,Published,ProcessId")] Memo memo)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if(memo.RecId == 0)
                    {
                        _DbContext.Add(memo);
                    }
                    else
                    {
                        var alreadyExist = _DbContext.Memo.AsNoTracking().SingleOrDefault(x => x.RecId == memo.RecId);

                        alreadyExist = memo;

                        _DbContext.Update(alreadyExist).Property(x => x.RecId).IsModified = false;

                    }

                    var check = await _DbContext.SaveChangesAsync();
                    if(check > 0)
                    {
                        TempData["message"] = "Memo saved successfully";
                        return RedirectToAction(nameof(Index));
                    }
                }



            }
            catch (DbUpdateConcurrencyException ex)
            {
                TempData["error"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            return View(memo);
        }

        // GET: Memos/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var memo = await _DbContext.Memo.FindAsync(id);
            if (memo == null)
            {
                return NotFound();
            }
            memo.ProcessList = _DbContext.Workflow.Select(a => new SelectListItem
            {
                Value = a.ProcessId.ToString(),
                Text = a.ProcessName
            }).ToList();

            return View(memo);
            //memo.MemoTypeList = _DbContext.MetaDataRef.Where(a => a.MetaDataType == Vars.MetaDataTypes["MEMO"]).Select(a => new SelectListItem
            //{
            //    Value = a.ReferenceId.ToString(),
            //    Text = a.ReferenceId
            //}).ToList();
            //TempData["UNASSIGNED"] = "UNASSIGNED";
        }

        // POST: Memos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("RecId,MemoName,MemoContent,DateCreated,Published,ProcessId")] Memo memo)
        {
            if (id != memo.RecId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _DbContext.Update(memo);
                    await _DbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemoExists(memo.RecId))
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
            return View(memo);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            try
            {
                //check if form already got entries
                _DbContext.Memo.Remove(_DbContext.Memo.FirstOrDefault(m => m.RecId == id));
                _DbContext.SaveChanges();

                TempData["message"] = "Memo deleted successfully";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Duplicate(int id)
        {
            try
            {
                var objMemo = _DbContext.Memo.FirstOrDefault(m => m.RecId == id);
                objMemo.RecId = 0;
                objMemo.Published = false;
                objMemo.DateCreated = DateTime.Now;
                string err = "";
                //if (objForm.UserId == _Vars.UserId)
                //{
                _DbContext.Memo.Add(objMemo);
                _DbContext.SaveChanges(true);
                err = "Duplication was successful";
                //}
                //else err = "Only the form creator is allowed to duplicate";

                TempData["message"] = err;
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool MemoExists(long id)
        {
            return _DbContext.Memo.Any(e => e.RecId == id);
        }
    }
}
