using System.Text.Json;

namespace AlpacaSpy.Models
{
    public class Mismatch(string propertyName, string mismatchMessage, string originalValue, string currentValue)
    {
        public string PropertyName { get; set; } = propertyName;
        public string MismatchMessage { get; set; } = mismatchMessage;
        public string OriginalValue { get; set; } = originalValue;
        public string CurrentValue { get; set; } = currentValue;
    }
}
