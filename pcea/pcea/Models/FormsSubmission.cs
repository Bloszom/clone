using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcea.Models
{
    [Table("FormsSubmission", Schema = "dbo")]
    public partial class FormsSubmission
    {
        public long RecId { get; set; }
        public int Submission { get; set; }
        public string FormName { get; set; }
        public string FormFields { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime DateCreated { get; set; }
        public bool Published { get; set; }
        public int? UpdateCount { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime LastUpdate { get; set; }
        public string FormType { get; set; }
        [StringLength(50)]
        public string FormsType { get; set; }
        [StringLength(50)]
        public string FormsTypeCategory { get; set; }
        public string UserId { get; set; }
        [StringLength(4)]
        public string FormYear { get; set; }
        [Column(TypeName = "datetime")]
        
        [NotMapped]
        public string TariffType { get; set; }
        [StringLength(50)]

        public DateTime TerminalDate { get; set; }
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
    }
}
