using Microsoft.AspNetCore.Mvc;
using pcea.Models;
using System.Linq;

namespace pcea.Controllers
{
    public class SysConfigController : Controller
    {
        PceaDbContext _DbContext;

        public SysConfigController(PceaDbContext context)
        {
            _DbContext = context;
        }

        public IActionResult Index()
        {
            var sysconfig = _DbContext.SystemConfig.FirstOrDefault();

            return View(sysconfig);
        }

        [HttpPost]
        public IActionResult Index(SystemConfig config)
        {
            _DbContext.SystemConfig.Update(config);
            _DbContext.SaveChanges(true);

            return RedirectToAction("Index");
        }
    }
}
