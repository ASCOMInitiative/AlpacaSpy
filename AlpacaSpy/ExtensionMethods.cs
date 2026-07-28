namespace AlpacaSpy
{
    public static class ExtensionMethods
    {
        public static LogLevel ToMSLogLevel(this ASCOM.Common.Interfaces.LogLevel logLevel)
        {
            switch (logLevel)
            {
                case ASCOM.Common.Interfaces.LogLevel.Verbose:
                case ASCOM.Common.Interfaces.LogLevel.Debug:
                    return LogLevel.Debug;

                case ASCOM.Common.Interfaces.LogLevel.Information:
                    return LogLevel.Information;

                case ASCOM.Common.Interfaces.LogLevel.Warning:
                    return LogLevel.Warning;

                case ASCOM.Common.Interfaces.LogLevel.Error:
                    return LogLevel.Error;

                default:
                    throw new ArgumentException($"Unknown logging level: {logLevel}");
            }
        }

        public static string ToRoundedString(this double value)
        {
            double abs = Math.Abs(value);
            return abs switch
            {
                < 1.0 => value.ToString("F3"),
                <= 100.0 => value.ToString("F2"),
                _ => value.ToString("F1")
            };
        }

        public static decimal ToDecimal(this double value)
        {
            return Convert.ToDecimal(value);
        }
    }
}
