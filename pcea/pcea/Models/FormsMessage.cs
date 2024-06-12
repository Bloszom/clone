using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcea.Models
{
    [Table("FormsMessage", Schema = "dbo")]
    public class FormsMessage
    {
        [Key]
        public long RecId { get; set; }
        public long EntryId { get; set; }
        public string OperatorId { get; set; }
        public string OperatorUserId { get; set; }
        public string OperatorEmail { get; set; }
        public string NccUserId { get; set; }
        public string NccEmail { get; set; }
        public string ProductName { get; set; }
        public string MessageBody { get; set; }
        public DateTime DateSent { get; set; }
        public string SentBy { get; set; }

        [NotMapped]
        public List<FormsMessage> Messages { get; set; }
    }
}
