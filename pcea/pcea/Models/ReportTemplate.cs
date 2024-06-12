using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace pcea.Models
{
    [Table("ReportTemplate", Schema = "dbo")]
    public partial class ReportTemplate
    {
        [Key]
        public long ReportId { get; set; }
        public long? FormId { get; set; } = 0;
        public string ReportType { get; set; }
        public string ReportName { get; set; }
        public string ReportSQL { get; set; }
        public string OrderBy { get; set; }
        public string FormFields { get; set; }
        public int? TableType { get; set; } = 0;

    }
}
