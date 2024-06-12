using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using pceaLibrary;
using pcea.Models;
using Microsoft.EntityFrameworkCore;

namespace pcea.Controllers
{
    public abstract class _BaseController : Controller
    {
        public List<Forms> FormList(PceaDbContext _DbContext, bool isTariffReq = false)
        {
            var resp = from c in _DbContext.Forms select c;
            
            //if (isTariffReq)
            //{
            //    resp.Where(s => s.OperatorType == "Tariff_request");
            //}
            //else
            //{
            //    resp.Where(s => s.OperatorType != "Tariff_request");
            //}

            return resp.ToList();
        }


    }
}
