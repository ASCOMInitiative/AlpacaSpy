namespace AlpacaSpy.Models
{
    /// <summary>
    /// Represents a mismatch between the expected and actual data in the AlpacaSpy application.
    /// </summary>
    public class MismatchSummary(DateTime mismatchTime, string sendLogMessage, string responseLogMessage, List<Mismatch> mismatches)
    {
        public DateTime MismatchTime { get; set; } = mismatchTime;
        public string SendLogMessage { get; set; } = sendLogMessage;
        public string ResponseLogMessage { get; set; } = responseLogMessage;
        public List<Mismatch> Mismatches { get; set; } = mismatches;
    }
}
