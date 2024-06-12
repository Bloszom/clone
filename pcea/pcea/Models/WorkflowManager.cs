using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcea.Models
{
    [Table("WorkflowManager", Schema = "dbo")]
    public partial class WorkflowManager
    {
        [Key]
        public long RecId { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Task Id")]
        public string TaskId { get; set; }
        [Required]
        [StringLength(20)]
        [Display(Name = "Task Type")]
        public string TaskType { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Process Id")]
        public string ProcessId { get; set; }
        [Required]
        [StringLength(500)]
        [Display(Name = "Reference No.")]
        public string ReferenceNo { get; set; }
        [StringLength(500)]
        [Display(Name = "Operator Name")]
        public string OperatorName { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Date Assigned")]
        public DateTime DateAssigned { get; set; }
        [StringLength(50)]
        [Display(Name = "Action Performed")]
        public string ActionId { get; set; }
        [StringLength(500)]
        [Display(Name = "Action Url")]
        public string ActionUrl { get; set; }
        [Required]
        [StringLength(500)]
        [Display(Name = "User Id")]
        public string UserId { get; set; }
        public bool IsSource { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Role")]
        public string RoleId { get; set; }
        [StringLength(500)]
        public string Remarks { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Date Completed")]
        public DateTime? DateCompleted { get; set; }
        [StringLength(3)]
        [Display(Name = "Completion Flag")]
        public string CompletionFlag { get; set; }
        [NotMapped]
        [Display(Name = "Forward To")]
        public string DestinationUserId { get; set; }
        [NotMapped]
        public string UplinkUserId { get; set; }
        [NotMapped]
        public string DownlinkUserId { get; set; }
        [NotMapped]
        [Display(Name = "User's Name")]
        public string AdminFullName { get; set; }        
        [NotMapped]
        [Display(Name = "Reopen")]
        public bool Reopen { get; set; }
        [NotMapped]
        public List<SelectListItem> ActionList { get; set; }
        [NotMapped]
        public List<SelectListItem> UserList { get; set; }
        [NotMapped]
        public List<SelectListItem> OperatorList { get; set; }
        [NotMapped]
        public List<SelectListItem> ProcessList { get; set; }
    }
}
