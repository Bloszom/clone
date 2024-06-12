using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcea.Models
{
    [Table("CompanyDataSubmission", Schema = "dbo")]
    public class CompanyDataSubmission
    {
        [Key]
        public long RecId { get; set; }
        [Required]
        [StringLength(500)]
        public string OrganizationId { get; set; }
        [Required]
        public long FormId { get; set; }
        [Required]
        public string FormFieldsData { get; set; }
    }
}
