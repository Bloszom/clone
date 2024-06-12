using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcea.Models
{
    [Table("TariffAnalysis", Schema = "dbo")]
    public partial class TariffAnalysis
    {
        [Key]
        public long RecId { get; set; }
        public string FieldId { get; set; }
        public string FieldName { get; set; }
        public long? EntryId { get; set; }
        public long? FormId { get; set; }
        [StringLength(50)]
        public string ParameterId { get; set; }
    }
}
