using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcea.Models
{
    [Table("SystemConfig", Schema = "dbo")]
    public partial class SystemConfig
    {
        [Key]
        public long RecId { get; set; }
        [StringLength(250)]
        public string SsoAppHost { get; set; }
        [StringLength(250)]
        public string SsoAppHostProfile { get; set; }
        [StringLength(250)]
        public string DocumentPath { get; set; }
        [StringLength(250)]
        public string ImagePath { get; set; }
        public string TariffReviewBase { get; set; }
        public string SurveyReviewBase { get; set; }
        public string OthersReviewBase { get; set; }
        public string TaskClosureAccount { get; set; }
        public string Secretary { get; set; }
    }
}
