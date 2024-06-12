using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcea.Models
{
    [Table("SsoToken", Schema = "dbo")]
    public partial class SsoToken
    {
        [Key]
        public long RecId { get; set; }
        public string TokenValue { get; set; }
        [StringLength(50)]
        public string TokenSource { get; set; }
        [Column("dateLogin", TypeName = "datetime")]
        public DateTime? DateLogin { get; set; }
    }
}
