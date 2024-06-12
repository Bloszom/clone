using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcea.Models
{
    [Table("Memo", Schema = "dbo")]
    public partial class Memo
    {
        [Key]
        public long RecId { get; set; }
        [Required]
        [StringLength(50)]
        public string MemoName { get; set; }
        //[Required]
        //[StringLength(50)]
        //public string MemoType { get; set; }
        [Required]
        public string MemoContent { get; set; }
        [Column(TypeName = "date")]
        public DateTime DateCreated { get; set; }
        public bool Published { get; set; }
        [Required]
        [StringLength(50)]
        public string ProcessId { get; set; }
        //[Required]
        //[StringLength(50)]
        //public string TaskId { get; set; }
        //------------------------------------------------//
        public string FormDate(DateTime dt)
        {
            try
            {
                if (dt == null) return "date not set";

                string nw = "";
                if ((DateTime.Now - dt).TotalDays >= 1)
                {
                    double nMth = (DateTime.Now - dt).TotalDays;
                    if (nMth > 30)
                    {
                        nw = "Over " + ((int)(nMth / 30)).ToString() + " month(s) ago";
                    }
                    else
                    {
                        nw = ((int)nMth).ToString() + " days(s) ago";
                    }
                }
                else if ((DateTime.Now - dt).TotalHours >= 1)
                {
                    nw = ((int)(DateTime.Now - dt).TotalHours).ToString() + " hour(s) ago";
                }
                else
                {
                    nw = ((int)(DateTime.Now - dt).TotalMinutes).ToString() + " minute(s) ago";
                }

                return nw;
            }
            catch (Exception)
            {
                return "date not set...";
            }
        }
        //[NotMapped]
        //public List<SelectListItem> MemoTypeList { get; set; }
        [NotMapped]
        public List<SelectListItem> ProcessList { get; set; }
    }

    public class GenerateMemoRequest
    {
        public long EntryId { get; set; }
        public string ProcessId { get; set; }
        public string ActionType { get; set; }
        public string ProductName { get; set; }
        public string OrganizationName { get; set; }
    }
}
