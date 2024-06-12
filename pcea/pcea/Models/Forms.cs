using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcea.Models
{
    [Table("Forms", Schema = "dbo")]
    public partial class Forms
    {
        [Key]
        public long RecId { get; set; }
        [Required]
        [StringLength(50)]
        public string FormName { get; set; }
        public string FormFields { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime DateCreated { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime TerminalDate { get; set; }
        public bool Published { get; set; }
        public int? UpdateCount { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime LastUpdate { get; set; }
        [StringLength(20)]
        public string FormType { get; set; }
        [StringLength(50)]
        public string UserId { get; set; }
        [StringLength(4)]
        public string FormYear { get; set; }
        [StringLength(50)]
        public string ProcessId { get; set; }
        [StringLength(50)]
        public string FormsType { get; set; }
        [StringLength(50)]
        public string FormsTypeCategory { get; set; }
        public string CompanyInfoFields { get; set; }
        public string FormDate(DateTime dt)
        {
            return new FormsSubmission().FormDate(dt);
        }

        [NotMapped]
        public List<SelectListItem> FormList { get; set; }
    }
}
