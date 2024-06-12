using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcea.Models
{
    public partial class TariffHistories
    {
        public long EntryIdMaster { get; set; }
        [StringLength(50)]
        public string OperatorId { get; set; }
        [StringLength(50)]
        public string FormName { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? DateCreated { get; set; }
        [StringLength(10)]
        public string AppType { get; set; }
        public string ProductName { get; set; }
        public string FormDetails { get; set; }
        public string Status { get; set; }
        public string FormsTypeCategory { get; set; }
    }
}
