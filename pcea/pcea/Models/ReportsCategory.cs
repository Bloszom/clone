using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcea.Models
{
    [Table("ReportsCategory", Schema = "dbo")]
    public partial class ReportsCategory
    {
        [Key]
        public long RecId { get; set; }
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
