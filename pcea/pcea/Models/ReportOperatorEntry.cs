using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcea.Models
{
    [Table("ReportOperatorEntry", Schema = "dbo")]
    public partial class ReportOperatorEntry
    {
        [Required]
        [StringLength(50)]
        public string OperatorId { get; set; }
        [StringLength(50)]
        public string FormsTypeCategory { get; set; }
        [StringLength(20)]
        public string FormType { get; set; }
        public int? TotalSubmission { get; set; }
        public int? YearOfSubmission { get; set; }
        [StringLength(50)]
        public string FormsType { get; set; }
    }
}
