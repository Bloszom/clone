using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcea.Models
{
    [Table("MetaData", Schema = "dbo")]
    public partial class MetaData
    {
        [Key]
        [StringLength(50)]
        public string MetaDataType { get; set; }
    }
}
