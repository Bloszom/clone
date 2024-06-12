using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace pcea.Models
{
    [Table("Workflow", Schema = "dbo")]
    public partial class Workflow
    {
        [Key]
        [StringLength(50)]
        public string ProcessId { get; set; }
        [Required]
        [RegularExpression(@"^[A-Z]+$", ErrorMessage = "Only block letters are allowed.")]
        [StringLength(50)]
        public string ProcessName { get; set; }
        [Column(TypeName = "date")]
        public DateTime DateCreated { get; set; }
        [Required]
        [StringLength(15)]
        public string Status { get; set; }
        [JsonRequired]
        public string ProcessData { get; set; }

    }
    public enum Status
    {
        Active,
        Inactive
    }
}
