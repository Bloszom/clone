namespace pcea.Models
{
    public class TariffHistory
    {
        //public string RecId { get; set; }
        public string OperatorName { get; set; }
        public string Category { get; set; }
        public string PlanName { get; set; }
        public string PlanTypeCategory { get; set; }
        public long PlanType { get; set; }
        public string ShortCode { get; set; }
        public string Description { get; set; }
        public string SubmissionDate { get; set; }
        public string Decision { get; set; }
        public string ConveyanceDate { get; set; }
        public string Processor { get; set; }
        public string ExpectedExpiryDate { get; set; }
        public string ExpectedLaunchDate { get; set; }
    }
}