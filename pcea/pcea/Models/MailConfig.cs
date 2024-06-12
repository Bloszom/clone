using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcea.Models
{
    [Table("MailConfig", Schema = "dbo")]
    public partial class MailConfig
    {
        [Key]
        [StringLength(250)]
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public int SslSupport { get; set; }
        [Required]
        [StringLength(250)]
        public string SmtpUsername { get; set; }
        [Required]
        [StringLength(100)]
        public string SmtpPassword { get; set; }
        [StringLength(250)]
        public string DefaultEmail { get; set; }
    }
}
