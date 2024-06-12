using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcea.Models
{
    [Table("SsoUser", Schema = "dbo")]
    public partial class SsoUser
    {
        [Key]
        public long RecId { get; set; }
        [Column("appUserId")]
        [StringLength(250)]
        public string AppUserId { get; set; }
        [Column("username")]
        [StringLength(250)]
        public string Username { get; set; }
        [Column("appUserEmail")]
        [StringLength(250)]
        public string AppUserEmail { get; set; }
        [Column("mobileNumber")]
        [StringLength(250)]
        public string MobileNumber { get; set; }
        [Column("userType")]
        [StringLength(250)]
        public string UserType { get; set; }
        [Column("dateCreated", TypeName = "datetime")]
        public DateTime? DateCreated { get; set; }
        [Column("firstName")]
        [StringLength(250)]
        public string FirstName { get; set; }
        [Column("lastName")]
        [StringLength(250)]
        public string LastName { get; set; }
        [Column("middleName")]
        [StringLength(250)]
        public string MiddleName { get; set; }
        [Column("active")]
        [StringLength(10)]
        public string Active { get; set; }
        [Column("allowedToUseApi")]
        [StringLength(10)]
        public string AllowedToUseApi { get; set; }
        [Column("emailVerified")]
        [StringLength(10)]
        public string EmailVerified { get; set; }
        [Column("phoneVerified")]
        [StringLength(10)]
        public string PhoneVerified { get; set; }
        [Column("image")]
        public string Image { get; set; }
        [Column("organizationId")]
        [StringLength(250)]
        public string OrganizationId { get; set; }
        [Column("organizationShortName")]
        public string OrganizationShortName { get; set; }
        [Column("organizationDescription")]
        public string OrganizationDescription { get; set; }
        [Column("organizationGroup")]
        public string OrganizationGroup { get; set; }
        [Column("organizationLongName")]
        public string OrganizationLongName { get; set; }
        [Column("logoPath")]
        public string LogoPath { get; set; }
        [Column("dateLogin", TypeName = "datetime")]
        public DateTime? DateLogin { get; set; }
        [Column("roleName")]
        [StringLength(250)]
        public string RoleName { get; set; }

        [Column("OtherInfoStreet")]
        [StringLength(500)]
        public string OtherInfoStreet { get; set; }

        [Column("OtherInfoCity")]
        [StringLength(500)]
        public string OtherInfoCity { get; set; }

        [Column("OtherInfoTelephone")]
        [StringLength(50)]
        public string OtherInfoTelephone { get; set; }

        [Column("OtherInfoFax")]
        [StringLength(50)]
        public string OtherInfoFax { get; set; }

        [Column("OtherInfoWebsite")]
        [StringLength(500)]
        public string OtherInfoWebsite { get; set; }

        [Column("OtherInfoEmail")]
        [StringLength(500)]
        public string OtherInfoEmail { get; set; }
    }
}
