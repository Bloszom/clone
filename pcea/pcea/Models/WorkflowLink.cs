using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace pcea.Models
{
    [Table("WorkflowLink", Schema = "dbo")]
    public class WorkflowLink
    {
        [Key]
        public long RecId { get; set; }
        [Required]
        [StringLength(50)]
        public string ProcessId { get; set; }
        [Required]
        public int LinkId { get; set; }
        [Required]
        public int FromActorNumber { get; set; }
        [Required]
        public int ToActorNumber { get; set; }

    }
}
