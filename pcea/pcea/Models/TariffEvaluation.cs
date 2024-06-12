using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcea.Models
{
    [Table("TariffEvaluation", Schema = "dbo")]
    public partial class TariffEvaluation
    {
        [Key]
        public long RecId { get; set; }
        public long FormId { get; set; }
        [Column("EntryID")]
        public long EntryId { get; set; }
        [Column(TypeName = "date")]
        public DateTime? StartDate { get; set; }
        [Column(TypeName = "date")]
        public DateTime? EndDate { get; set; }
    }
    public enum Parameters
    {
        MainAccount,
        DataAccount,
        VoiceBonus,
        DataBonus,
        BurnRateMainAccount,
        BurnRateDataAccount,
        BurnRateVoiceBonus,
        BurnRateDataBonus
    }
}
