using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcea.Models
{
    [Table("FormsOperators", Schema = "dbo")]
    public partial class FormsOperators
    {
        public long FormId { get; set; }
        public long EntryId { get; set; }
        public string OperatorId { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime DateSubmitted { get; set; }
        public string Status { get; set; }
        public string FormDate(DateTime dt)
        {
            return new FormsSubmission().FormDate(dt);
        }
    }
}
