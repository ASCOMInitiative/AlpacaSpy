using AlpacaSpy.Models;
using ASCOM.Tools;
using Microsoft.AspNetCore.Http.HttpResults;
using Radzen.Documents.Markdown;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AlpacaSpy
{
    /// <summary>
    /// ASP.NET Core middleware that intercepts all Alpaca device API requests (/api/v1/...),
    /// logs the raw HTTP traffic, and forwards them to the actual device.
    /// Management API (/management/...) and Blazor routes pass through unmodified.
    /// </summary>
    public class AlpacaProxyMiddleware(RequestDelegate next, IHttpClientFactory httpClientFactory, State state, AlpacaSpyLogger logger)
    {
        private static readonly Regex DeviceApiPattern =
            new(@"^/api/v1/([^/]+)/(\d+)/([^?]*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Canonical Alpaca URL path segment → enum mapping (case-insensitive lookup)
        private static readonly Dictionary<string, AlpacaDeviceType> DeviceTypeMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["camera"] = AlpacaDeviceType.Camera,
                ["covercalibrator"] = AlpacaDeviceType.CoverCalibrator,
                ["dome"] = AlpacaDeviceType.Dome,
                ["filterwheel"] = AlpacaDeviceType.FilterWheel,
                ["focuser"] = AlpacaDeviceType.Focuser,
                ["observingconditions"] = AlpacaDeviceType.ObservingConditions,
                ["rotator"] = AlpacaDeviceType.Rotator,
                ["safetymonitor"] = AlpacaDeviceType.SafetyMonitor,
                ["switch"] = AlpacaDeviceType.Switch,
                ["telescope"] = AlpacaDeviceType.Telescope,
            };

        // Headers that must not be forwarded between hops
        private static readonly HashSet<string> HopByHopHeaders = new(StringComparer.OrdinalIgnoreCase)
        {
            "Connection", "Keep-Alive", "Proxy-Authenticate", "Proxy-Authorization", "TE", "Trailers", "Transfer-Encoding", "Upgrade", "Host"
        };

        private static readonly JsonSerializerOptions PascalCaseOptions = new()
        {
            PropertyNamingPolicy = null  // preserve PascalCase as required by Alpaca protocol
        };

        public async Task InvokeAsync(HttpContext context)
        {
            string path = context.Request.Path.Value ?? string.Empty;
            Match match = DeviceApiPattern.Match(path);

            // Only process paths that match the Alpaca API protocol, other paths are passed to the rest of the pipeline for processing
            if (!match.Success)
            {
                await next(context);
                return;
            }

            string deviceTypeStr = match.Groups[1].Value;
            string method = match.Groups[3].Value;

            // Check whether the given device type is valid
            if (!DeviceTypeMap.TryGetValue(deviceTypeStr, out AlpacaDeviceType deviceType))
            {
                // Unknown device type - let ASCOM.Alpaca.Razor return the error
                await next(context);
                return;
            }

            // Check whether the device number matches one we are proxying
            if (!int.TryParse(match.Groups[2].Value, out int proxyDeviceNumber))
            {
                await next(context);
                return;
            }

            // Get the specified device
            ConfiguredDevice? device = state.ConfiguredDevices.FirstOrDefault(d =>
                d.DeviceType == deviceType && d.ProxyDeviceNumber == proxyDeviceNumber);

            // Validate that we got a device i.e. that we are proxying the specified device
            if (device == null) // We are not proxying this device so reject the request
            {
                await ReturnAlpacaErrorAsync(context, 1024, $"AlpacaSpy: no configured device for {deviceTypeStr}/{proxyDeviceNumber}");
                return;
            }

            // Read and buffer the request body (needed for both logging and forwarding)
            byte[] requestBodyBytes = await ReadRequestBodyAsync(context);

            SelectiveLogMemberType memberType = SelectiveLoggingMetadata.ResolveMemberType(deviceType, method, context.Request.Method);
            SelectiveLogMember member = new(method, memberType);
            bool logThisCall = SelectiveLoggingMetadata.IsMemberEnabled(device, member);

            // Log client request to AlpacaSpy
            if (logThisCall)
                LogClientRequest(context, device, requestBodyBytes, method);

            // Build the forwarding URL, translating the proxy device number to the real device number
            string targetUrl = $"http://{device.IpAddress}:{device.PortNumber}/api/v1/{deviceTypeStr}/{device.RemoteDeviceNumber}/{method}{context.Request.QueryString.Value}";

            HttpClient client = httpClientFactory.CreateClient("AlpacaProxy");
            using HttpRequestMessage forwardRequest = BuildForwardRequest(context, targetUrl, requestBodyBytes);

            // Send the message to the device and wait for its response.
            HttpResponseMessage responseMessage;
            byte[] responseBytes;
            try
            {
                DateTime requestSentToDevice = DateTime.Now;
                responseMessage = await client.SendAsync(forwardRequest, HttpCompletionOption.ResponseContentRead);
                responseBytes = await responseMessage.Content.ReadAsByteArrayAsync();
                DateTime responseFromDevice = DateTime.Now;
                if (device.Recording)
                {
                    AlpacaTransaction transaction = new AlpacaTransaction()
                    {
                        RequestMethod = context.Request.Method,
                        RequestUri = context.Request.Path + context.Request.QueryString,
                        RequestHeaders = device.RecordClientHeaders ? context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToArray<string>()) : new(),
                        RequestHttpVersion = context.Request.Protocol switch
                        {
                            "HTTP/1.0" => new Version(1, 0),
                            "HTTP/1.1" => new Version(1, 1),
                            "HTTP/2" => new Version(2, 0),
                            _ => new Version(1, 1)
                        },
                        RequestBody = requestBodyBytes.Length > 0 ? Encoding.UTF8.GetString(requestBodyBytes) : "",
                        RequestTimeSent = requestSentToDevice,
                        ResponseStatusCode = responseMessage.StatusCode,
                        ResponseHttpVersion = responseMessage.Version,
                        ResponseHeaders = device.RecordDeviceHeaders ? responseMessage.Headers.ToDictionary(h => h.Key, h => h.Value.ToArray<string>()) : new(),
                        ResponseTime = responseFromDevice - requestSentToDevice,
                    };

                    string responseString = responseBytes.Length > 0 ? Encoding.UTF8.GetString(responseBytes) : "";
                    try
                    {
                        JsonDocument document = JsonDocument.Parse(responseString);
                        transaction.Response = document;
                    }
                    catch // The supplied string was not parseable into a JSON string so create a valid JSON document that contains the response string as a plain string value
                    {
                        // Ensure that this reporting mechanic does not interrupt recording if it fails.
                        try
                        {
                            // Create a JSON string containing the response string as a property value. 
                            // NOTE: This tricky piece of code works by creating a temporary anonymous object, assigning responseString's value to its property "UnparsableResponse" and then serializing that object into a JSON string.
                            string json = JsonSerializer.Serialize(new
                            {
                                UnparsableResponse = responseString
                            });

                            // Parse the JSON string into a JSON document and save it
                            JsonDocument document = JsonDocument.Parse(json);
                            transaction.Response = document;
                        }
                        catch (Exception ex)
                        {
                            logger.LogError("AlpacaProxyMiddleware", $"Error parsing response: {responseString}\r\n{ex}");
                        }
                    }

                    // Save the transaction for later replaying
                    state.Transactions[device].Add(transaction);
                }
            }
            catch (Exception ex)
            {
                if (logThisCall)
                {
                    logger.LogError("Proxy", $"Forward error → {device.Name} ({targetUrl}): {ex.Message}");
                    logger.LogDebug("Proxy", ex.ToString());
                }
                await ReturnAlpacaErrorAsync(context, 1024, $"AlpacaSpy proxy error: {ex.Message}");
                return;
            }

            // Dispose responseMessage promptly once we are done with it 
            using (responseMessage)
            {
                // Log device's response
                if (logThisCall)
                    LogDeviceResponse(responseMessage, responseBytes, device);

                // Forward response to the original client

                // Set the HTTP status
                context.Response.StatusCode = (int)responseMessage.StatusCode;

                // Add the response headers
                foreach (KeyValuePair<string, IEnumerable<string>> header in responseMessage.Headers)
                {
                    if (!HopByHopHeaders.Contains(header.Key))
                        context.Response.Headers[header.Key] = header.Value.ToArray();
                }

                // Add response content headers
                foreach (KeyValuePair<string, IEnumerable<string>> header in responseMessage.Content.Headers)
                {
                    if (!HopByHopHeaders.Contains(header.Key))
                        context.Response.Headers[header.Key] = header.Value.ToArray();
                }

                // Send the response to the client.
                await context.Response.Body.WriteAsync(responseBytes);
            }
        }

        private static async Task<byte[]> ReadRequestBodyAsync(HttpContext context)
        {
            if (!context.Request.Method.Equals("PUT", StringComparison.OrdinalIgnoreCase))
                return [];

            context.Request.EnableBuffering();
            using MemoryStream ms = new();
            await context.Request.Body.CopyToAsync(ms);
            context.Request.Body.Position = 0;
            return ms.ToArray();
        }

        private static HttpRequestMessage BuildForwardRequest(HttpContext context, string targetUrl, byte[] bodyBytes)
        {
            HttpRequestMessage request = new()
            {
                Method = new HttpMethod(context.Request.Method),
                RequestUri = new Uri(targetUrl)
            };

            // Collect headers rejected by HttpRequestMessage.Headers (i.e. content headers) so they
            // can be applied to request.Content after the body is assigned — not before (fix: was null).
            List<(string Key, string?[] Values)> deferredContentHeaders = [];
            foreach (KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues> header in context.Request.Headers)
            {
                if (HopByHopHeaders.Contains(header.Key)) continue;
                if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
                {
                    if (header.Value.Count > 0)
                        deferredContentHeaders.Add((header.Key, header.Value.ToArray()));
                }
            }

            if (bodyBytes.Length > 0)
            {
                request.Content = new ByteArrayContent(bodyBytes);
                foreach ((string Key, string?[] Values) deferredHeader in deferredContentHeaders)
                {
                    string key = deferredHeader.Key;
                    string?[] values = deferredHeader.Values;
                    request.Content.Headers.TryAddWithoutValidation(key, values);
                }
            }

            return request;
        }

        private void LogClientRequest(
            HttpContext context, ConfiguredDevice device, byte[] bodyBytes, string method)
        {
            StringBuilder sb = new();
            sb.Append($"──► {context.Request.Method} /api/v1/{device.DeviceType.ToString().ToLower()}/{device.ProxyDeviceNumber}/{method}{context.Request.QueryString}  [{device.Name}]");

            if (bodyBytes.Length > 0)
            {
                sb.Append("  BODY PARAMETERS:");
                foreach (string part in Encoding.UTF8.GetString(bodyBytes).Split('&'))
                    sb.Append($" {part},");
            }
            sb.AppendLine();

            if (device.LogClientHeaders)
            {
                sb.AppendLine("  SENDING HEADERS TO DEVICE:");
                foreach (KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues> h in context.Request.Headers)
                    sb.AppendLine($"    {h.Key}: {h.Value}");
            }

            if (device.LogClientParams && context.Request.QueryString.HasValue)
            {
                sb.AppendLine("  SENDING QUERY PARAMETERS TO DEVICE:");
                foreach (KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues> q in context.Request.Query)
                    sb.AppendLine($"    {q.Key} = {q.Value}");
            }
            if (device.LogClientParams && bodyBytes.Length > 0)
            {
                sb.AppendLine("  SENDING BODY PARAMETERS TO DEVICE:");
                foreach (string part in Encoding.UTF8.GetString(bodyBytes).Split('&'))
                    sb.AppendLine($"    {part},");
            }

            WriteToLogs(device, sb.ToString().TrimEnd());
        }

        private void LogDeviceResponse(HttpResponseMessage response, byte[] responseBytes, ConfiguredDevice device)
        {
            string? contentType = response.Content.Headers.ContentType?.ToString().ToLowerInvariant().Split(";").First().Trim();
            string text;
            StringBuilder sb = new();
            sb.Append($"◄── {(int)response.StatusCode} {response.ReasonPhrase} - ");

            if (responseBytes.Length > 0)
            {
                switch (contentType)
                {
                    case "application/imagebytes": // Handle imagebytes as a hex dump of the first 50 bytes to avoid log flooding
                        sb.Append($"-");
                        for (int i = 0; i < Math.Min(50, responseBytes.Length); i++)
                        {
                            sb.Append($" [{responseBytes[i]:X2}]");
                        }
                        sb.AppendLine();
                        break;

                    default: // For all other content types, log the first 500 characters of the response text
                        text = Encoding.UTF8.GetString(responseBytes);
                        sb.AppendLine($"{text[..Math.Min(500, text.Length)]}");
                        break;
                }
            }

            if (device.LogDeviceHeaders)
            {
                sb.AppendLine("  DEVICE RESPONSE HEADERS:");
                foreach (KeyValuePair<string, IEnumerable<string>> h in response.Headers)
                    sb.AppendLine($"    {h.Key}: {string.Join(", ", h.Value)}");
                foreach (KeyValuePair<string, IEnumerable<string>> h in response.Content.Headers)
                    sb.AppendLine($"    {h.Key}: {string.Join(", ", h.Value)}");
            }
            if (device.LogJsonParameters && responseBytes.Length > 0 && contentType != "application/imagebytes")
            {
                sb.AppendLine($"  JSON NAME-VALUE PAIRS:");
                try
                {
                    using JsonDocument doc = JsonDocument.Parse(responseBytes);
                    foreach (JsonProperty prop in doc.RootElement.EnumerateObject())
                        sb.AppendLine($"    {prop.Name}: {prop.Value}");
                }
                catch (JsonException ex)
                {
                    sb.AppendLine($"    Invalid JSON detected: {ex.Message}");
                    sb.AppendLine($"      Response: {Encoding.UTF8.GetString(responseBytes)}");
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"    Unable to parse device response: {Encoding.UTF8.GetString(responseBytes)}");
                    sb.AppendLine($"      Exception: {ex.Message}");
                }
            }

            WriteToLogs(device, $"{sb.ToString().TrimEnd()}\r\n");
        }

        private void WriteToLogs(ConfiguredDevice device, string message)
        {
            logger.LogMessage(device.Name, message);

            if (state.DeviceLoggers.TryGetValue(device.UniqueId, out TraceLogger? traceLogger) && traceLogger is not null)
                traceLogger.LogMessage("Proxy", message);
        }

        private async Task ReturnAlpacaErrorAsync(
            HttpContext context, int errorNumber, string errorMessage, int statusCode = 500)
        {
            // Parse ClientTransactionID from query string so the client can correlate errors
            int clientTxId = 0;
            if (context.Request.Query.TryGetValue("ClientTransactionID", out Microsoft.Extensions.Primitives.StringValues ctStr))
                int.TryParse(ctStr, out clientTxId);

            object error = new
            {
                ClientTransactionID = clientTxId,
                ServerTransactionID = (int)state.GetServerTransactionId(),
                ErrorNumber = errorNumber,
                ErrorMessage = errorMessage
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(error, PascalCaseOptions));
        }
    }
}
