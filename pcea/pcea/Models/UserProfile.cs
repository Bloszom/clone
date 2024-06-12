using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcea.Models
{
    [Table("UserProfile", Schema = "dbo")]
    public partial class UserProfile
    {
        [Key]
        [StringLength(500)]
        public string UserId { get; set; }
        [Required]
        [StringLength(500)]
        public string Fullname { get; set; }
        [StringLength(50)]
        public string JobTitle { get; set; }
        [StringLength(20)]
        public string Telephone { get; set; }
        [Required]
        [StringLength(250)]
        public string Email { get; set; }
        [Required]
        [StringLength(50)]
        public string RoleId { get; set; }
        [StringLength(500)]
        public string ImageUrl { get; set; }
        [Column("_Password")]
        [StringLength(20)]
        public string Password { get; set; }
        [StringLength(15)]
        public string Status { get; set; }
        [StringLength(15)]
        public string UserType { get; set; }
        [StringLength(500)]
        public string OrganizationId { get; set; }
        [StringLength(500)]
        public string OrganizationName { get; set; }
        [Column(TypeName = "date")]
        public DateTime? DateCreated { get; set; }
        [Column(TypeName = "date")]
        public DateTime? DateLastLogin { get; set; }
        [StringLength(250)]
        public string AppUserId { get; set; }
        [NotMapped]
        public Dictionary<string, string> StatusList { get; set; }
        [NotMapped]
        public List<SelectListItem> RoleList { get; set; }
    }
}
