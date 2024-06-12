using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcea.Models
{
    [Table("WorkflowActor", Schema = "dbo")]
    public partial class WorkflowActor
    {
        [Key]
        public long RecId { get; set; }
        [Required]
        [StringLength(50)]
        public string ProcessId { get; set; }
        [Required]
        public int ActorNumber { get; set; }
        [Required]
        [StringLength(50)]
        public string ActorName { get; set; }
    }
}
