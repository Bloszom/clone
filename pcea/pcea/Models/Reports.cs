using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcea.Models
{
    [Table("Reports", Schema = "dbo")]
    public partial class Reports
    {
        [Key]
        public int RecId { get; set; }
        [Required]
        public int Category { get; set; }
        [Required]
        [StringLength(50)]
        public string ReportName { get; set; }
        [StringLength(50)]
        public string ReportColumnType { get; set; }
        [StringLength(250)]
        public string ReportColumnName { get; set; }
        [StringLength(250)]
        public string ColumnColor { get; set; }
        [StringLength(50)]
        public string ReportRowType { get; set; }
        [StringLength(250)]
        public string ReportRowName { get; set; }
        public string ReportQuery { get; set; }
        [StringLength(20)]
        public string ChartType { get; set; }
        [StringLength(50)]
        public string CreatedBy { get; set; }
        public string AnalysisFields { get; set; }
        [StringLength(20)]
        public string AnalysisAggregator { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime DateCreated { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime DateUpdated { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime DateMigrated { get; set; }

    }
}
