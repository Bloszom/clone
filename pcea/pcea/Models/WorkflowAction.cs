using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcea.Models
{
    [Table("WorkflowAction", Schema = "dbo")]
    public partial class WorkflowAction
    {
        [Key]
        public long RecId { get; set; }
        [Required]
        [StringLength(50)]
        public string ActionId { get; set; }
        [Required]
        [StringLength(50)]
        public string ProcessId { get; set; }
    }
}
