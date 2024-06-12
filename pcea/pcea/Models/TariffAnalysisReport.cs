using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcea.Models
{
    [Table("TariffAnalysisReport", Schema = "dbo")]
    public partial class TariffAnalysisReport
    {
        [Key]
        public long RecId { get; set; }
        public long EntryId { get; set; }
        public long FormId { get; set; }
        [Required]
        public string ParameterId { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Amount { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? BurnRate { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? EffectiveTariff { get; set; }
    }
}
