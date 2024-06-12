using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcea.Models
{
    [Table("Analysis", Schema = "dbo")]
    public partial class Analysis
    {
        [Key]
        public long RecId { get; set; }
        [Required]
        public int ReportId { get; set; }
        [Required]
        [Column(TypeName = "datetime")]
        public DateTime ReportDate { get; set; }
        public decimal ReportValue { get; set; }
        [Required]
        [StringLength(150)]
        public string Operator { get; set; }
        [StringLength(20)]
        public string OperatorType { get; set; }
        [StringLength(50)]
        public string Analyst { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime EntryDate { get; set; }
        [StringLength(4)]
        public string Year { get; set; }

    }
}
