using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace pcea.Models
{
    [Table("FormsAndEntry", Schema = "dbo")]
    public partial class FormsAndEntry
    {
        public long RecId { get; set; }
        public string FormName { get; set; }
        public string FormFields { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime DateCreated { get; set; }
        public bool Published { get; set; }
        public int? UpdateCount { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime LastUpdate { get; set; }
        [StringLength(20)]
        public string FormType { get; set; }
        [StringLength(4)]
        public string FormYear { get; set; }
        [StringLength(50)]
        public string FormsType { get; set; }
        [StringLength(50)]
        public string FormsTypeCategory { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime TerminalDate { get; set; }
        [StringLength(50)]
        public string ProcessId { get; set; }
        [StringLength(50)]
        public string OperatorId { get; set; }
        public string OrganizationName { get; set; }
        public long EntryId { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime DateSubmitted { get; set; }
        [StringLength(20)]
        public string Status { get; set; }
        [StringLength(50)]
        public string UserId { get; set; }
        [StringLength(10)]
        public string ReviewStatus { get; set; }
        [StringLength(500)]
        public string ReviewRemarks { get; set; }
        public string ReviewFileUrl { get; set; }
        [Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime? DateEvaluated { get; set; }
        [StringLength(250)]
        public string Message { get; set; }
        public string AppType { get; set; }
        public string ProductName { get; set; }
        public long? OldEntryId { get; set; }
        public string ProductConcept { get; set; }
        public string EvaluationRemarks { get; set; }
        public string EvaluationFileUrl { get; set; }
        public long? MasterEntryId { get; set; }
        public DateTime? DateofConveyance { get; set; }
        public DateTime? LaunchDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string MemoContent { get; set; }
        [NotMapped]
        public List<SelectListItem> MemoList { get; set; }

    }
}
