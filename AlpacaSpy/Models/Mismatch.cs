namespace AlpacaSpy.Models
{
    /// <summary>
    /// Represents a mismatch between the expected and actual data in the AlpacaSpy application.
    /// </summary>
    public class Mismatch(DateTime mismatchTime, string sendLogMessage, string responseLogMessage, string originalJsonString, string currentJsonString, string mismatchMessage, string propertyName)
    {
        public DateTime MismatchTime { get; set; } = mismatchTime;
        public string SendLogMessage { get; set; } = sendLogMessage;
        public string ResponseLogMessage { get; set; } = responseLogMessage;
        public string OriginalJsonString { get; set; } = originalJsonString;
        public string CurrentJsonString { get; set; } = currentJsonString;
        public string MismatchMessage { get; set; } = mismatchMessage;
        public string PropertyName { get; set; } = propertyName;
    }
}
