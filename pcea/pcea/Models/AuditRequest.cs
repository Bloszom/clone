using pcea.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pcea.Models
{
    public class AuditRequest
    {
        public string Name { get; set; }
        public List<SelectListItem> NameList { get; set; }
        public List<LogItem> LogItems { get; set; }
    }
}
