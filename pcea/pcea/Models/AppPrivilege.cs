using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcea.Models
{
    [Table("AppPrivilege", Schema = "dbo")]
    public partial class AppPrivilege
    {
        [Key]
        public long PrivilegeId { get; set; }
        [Required]
        [StringLength(100)]
        public string Description { get; set; }
        [Required]
        [StringLength(100)]
        public string ControllerName { get; set; }
        [Required]
        [StringLength(100)]
        public string ActionName { get; set; }
        [NotMapped]
        public bool Assigned { get; set; } = false;
    }
}
