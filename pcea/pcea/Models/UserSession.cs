using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcea.Models
{
    [Table("UserSession", Schema = "dbo")]
    public partial class UserSession
    {
        [Key]
        public long RecId { get; set; }
        [Required]
        [StringLength(250)]
        public string LoginToken { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime LoginDate { get; set; }
        [Required]
        [StringLength(250)]
        public string SessionVariable { get; set; }
        [Column(TypeName = "text")]
        public string SessionValue { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? LogoutDate { get; set; }
    }
}
