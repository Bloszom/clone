using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcea.Models
{
    [Table("MetaDataRef", Schema = "dbo")]
    public partial class MetaDataRef
    {
        [Key]
        public long RecId { get; set; }
        [Required]
        [StringLength(50)]
        public string ReferenceId { get; set; }
        [Required]
        [StringLength(50)]
        public string MetaDataType { get; set; }

    }

}
