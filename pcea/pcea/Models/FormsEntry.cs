using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;

namespace pcea.Models
{
    [Table("FormsEntry", Schema = "dbo")]
    public partial class FormsEntry
    {
        [Key]
        public long RecId { get; set; }
        [Required]
        public long FormId { get; set; }
        [Required]
        [StringLength(50)]
        public string FieldName { get; set; }
        public string Response { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime DateSubmitted { get; set; }
        [StringLength(50)]
        public string OperatorId { get; set; }
        [StringLength(50)] 
        public string UserId { get; set; }
        [Required]
        public long EntryId { get; set; }
        [StringLength(20)]
        public string Status { get; set; }
        [StringLength(500)]
        public string FieldLabel { get; set; }
        public bool IsFile { get; set; }
        [Display(Name = "Form Year")]
        [StringLength(4)]
        public string FrmYear { get; set; }        
        [Display(Name = "Form Year")]
        [StringLength(4)]
        public string ValueYear { get; set; }

    }
}
