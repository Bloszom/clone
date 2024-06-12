using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcea.Models
{
    [Table("AppRole", Schema = "dbo")]
    public class AppRole
    {
        [Key]
        [StringLength(50)]
        public string RoleId { get; set; }
        [Required]
        [StringLength(50)]
        public string RoleName { get; set; }

        public int RoleNo { get; set; }
    }
}
