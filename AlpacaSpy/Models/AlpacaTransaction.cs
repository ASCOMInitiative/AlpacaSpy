using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AlpacaSpy.Models
{
    public class AlpacaTransaction
    {
        // Request properties
        public string RequestUri { get; set; } = string.Empty;
        public string RequestMethod { get; set; } = HttpMethod.Get.Method;
        public Version RequestHttpVersion { get; set; } = new Version();
        public Dictionary<string, string[]> RequestHeaders { get; set; } = new Dictionary<string, string[]>();
        public string RequestBody { get; set; } = "";
        public DateTime RequestTimeSent { get; set; } = DateTime.MinValue;

        // Response properties
        public HttpStatusCode ResponseStatusCode { get; set; }
        public JsonDocument? Response { get; set; }
        public Version ResponseHttpVersion { get; set; } = new Version();
        public Dictionary<string, string[]> ResponseHeaders { get; set; } = new Dictionary<string, string[]>();
        public TimeSpan ResponseTime { get; set; } = TimeSpan.Zero;
    }
}
