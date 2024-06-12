using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcea.Models
{
    [Table("MailMessage", Schema = "dbo")]
    public partial class MailMessage
    {
        [Key]
        public long RecId { get; set; }
        [Required]
        [StringLength(50)]
        public string ReferenceNo { get; set; }
        [Required]
        [StringLength(250)]
        public string MailFrom { get; set; }
        [Required]
        [Column(TypeName = "text")]
        public string MailTo { get; set; }
        [Required]
        [StringLength(250)]
        public string MailSubject { get; set; }
        [Required]
        [Column(TypeName = "text")]
        public string MailBody { get; set; }
        [StringLength(50)]
        public string MailType { get; set; }
        [Column(TypeName = "date")]
        public DateTime DateCreated { get; set; }
    }
}
