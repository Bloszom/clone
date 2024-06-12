using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcea.Models
{
    [Table("UserPrivilege", Schema = "dbo")]
    public partial class UserPrivilege
    {
        [Key]
        public long RecId { get; set; }
        public long PrivilegeId { get; set; }
        [Required]
        [StringLength(50)]
        public string RoleId { get; set; }
        public bool Assigned { get; set; }
    }
}
