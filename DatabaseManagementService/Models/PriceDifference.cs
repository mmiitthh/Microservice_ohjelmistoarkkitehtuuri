namespace DatabaseManagementService.Models
{
    public class PriceDifference
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public double PriceDifferenceValue { get; set; }

        public PriceDifference(DateTime start, DateTime end, double priceDifferenceValue)
        {
            Start = start;
            End = end;
            PriceDifferenceValue = priceDifferenceValue;
        }
    }
}
