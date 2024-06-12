using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcea.Models
{
    [Table("AppUserProfileViewV2", Schema = "dbo")]
    public partial class AppUserProfileViewV2
    {
        public string UserId { get; set; }
        public string Fullname { get; set; }
        public string JobTitle { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string RoleId { get; set; }
        public string ImageUrl { get; set; }
        public string _Password { get; set; }
        public string Status { get; set; }
        public string UserType { get; set; }
        public string OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public string OrganizationLongName { get; set; }
        public string OrganizationShortName { get; set; }
        [Column(TypeName = "date")]
        public DateTime? DateCreated { get; set; }
        //public DateTime? DateLastLogin { get; set; }

        public string Username { get; set; }

        public string OtherInfoStreet { get; set; }

        public string OtherInfoCity { get; set; }

        public string OtherInfoTelephone { get; set; }

        public string OtherInfoFax { get; set; }

        public string OtherInfoWebsite { get; set; }

        public string OtherInfoEmail { get; set; }
        [NotMapped]
        public Dictionary<string, string> StatusList { get; set; }
        [NotMapped]
        public List<SelectListItem> RoleList { get; set; }
    }
}
