using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcea.Models
{
    [Table("FormsReview", Schema = "dbo")]
    public partial class FormsReview
    {
        [Key]
        public long RecId { get; set; }
        public long EntryId { get; set; }
        [Required]
        [StringLength(18)]
        public string Status { get; set; }
        [StringLength(500)]
        public string Remarks { get; set; }
        public string FileUrl { get; set; }
        public string EvaluationRemarks { get; set; }
        public string EvaluationFileUrl { get; set; }
        [NotMapped]
        public string? Processor { get; set; }
        [NotMapped]
        public string? ShortCode { get; set; }
        public DateTime? DateofConveyance { get; set; }
        public DateTime? LaunchDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public string MemoContent { get; set; }
        [NotMapped]
        public string DataType { get; set; }
        [NotMapped]
        public IFormFile File { get; set; }
    }
}
