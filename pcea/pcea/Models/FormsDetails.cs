using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcea.Models
{
    [Table("FormsDetails", Schema = "dbo")]
    public partial class FormsDetails
    {
        [Key]
        public long RecId { get; set; }
        [Required]
        [StringLength(50)]
        public string OperatorId { get; set; }
        public long FormId { get; set; }
        public string FormDetails { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime DateCreated { get; set; }
        [StringLength(250)]
        public string Message { get; set; }
        public long EntryId { get; set; }
        [StringLength(20)]
        public string Status { get; set; }
        [StringLength(20)]
        public string AppType { get; set; }
        public long? MasterEntryId { get; set; }
        public long? OldEntryId { get; set; }
        public string ProductName { get; set; }
        public string ProductConcept { get; set; }
        public string ExportDetails { get; set; }
        public string ExportLabels { get; set; }
        public string LicenseCategory { get; set; }
        public string FormDate(DateTime dt)
        {
            return new FormsSubmission().FormDate(dt);
        }
    }
}
