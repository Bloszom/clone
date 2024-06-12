using Microsoft.AspNetCore.Mvc.Rendering;
using pcea.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pcea.Models
{
    public class AuditTrailObj
    {
        public List<SelectListItem> NameList { get; set; }
        public List<LogItem > LogItems { get; set; }
        public string Name { get; set; }
    }
}
