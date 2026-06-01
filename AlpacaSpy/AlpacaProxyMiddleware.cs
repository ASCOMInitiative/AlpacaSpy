using AlpacaSpy.Models;
using ASCOM.Tools;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

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
            "Connection", "Keep-Alive", "Proxy-Authenticate", "Proxy-Authorization",
            "TE", "Trailers", "Transfer-Encoding", "Upgrade", "Host"
        };

        private static readonly JsonSerializerOptions PascalCaseOptions = new()
        {
            PropertyNamingPolicy = null  // preserve PascalCase as required by Alpaca protocol
        };

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            var match = DeviceApiPattern.Match(path);

            // Only process paths that match the Alpaca API protocol, other paths are passed to the rest of the pipeline for processing
            if (!match.Success)
            {
                await next(context);
                return;
            }

            string deviceTypeStr = match.Groups[1].Value;
            string method = match.Groups[3].Value;

            // Check whether the given device type is valid
            if (!DeviceTypeMap.TryGetValue(deviceTypeStr, out var deviceType))
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
                await ReturnAlpacaErrorAsync(context, 1024,
                    $"AlpacaSpy: no configured device for {deviceTypeStr}/{proxyDeviceNumber}");
                return;
            }

            // Read and buffer the request body (needed for both logging and forwarding)
            byte[] requestBodyBytes = await ReadRequestBodyAsync(context);

            // Log client request to AlpacaSpy
            LogClientRequest(context, device, requestBodyBytes, method);

            // Build the forwarding URL, translating the proxy device number to the real device number
            string targetUrl = $"http://{device.IpAddress}:{device.PortNumber}/api/v1/{deviceTypeStr}/{device.RemoteDeviceNumber}/{method}{context.Request.QueryString.Value}";

            var client = httpClientFactory.CreateClient("AlpacaProxy");
            using HttpRequestMessage forwardRequest = BuildForwardRequest(context, targetUrl, requestBodyBytes);

            foreach (var header in forwardRequest.Headers)
            {
                logger.LogDebug("", $"Sending header {header.Key} = ");
                foreach (var item in header.Value)
                {
                    logger.LogDebug("", $"Containing value  {item}");
                }
            }
            HttpResponseMessage responseMessage;
            byte[] responseBytes;
            try
            {
                responseMessage = await client.SendAsync(forwardRequest, HttpCompletionOption.ResponseContentRead);
                responseBytes = await responseMessage.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                logger.LogError("Proxy", $"Forward error → {device.Name} ({targetUrl}): {ex.Message}");
                await ReturnAlpacaErrorAsync(context, 1024, $"AlpacaSpy proxy error: {ex.Message}");
                return;
            }

            // Dispose responseMessage promptly once we are done with it (fix: was previously leaked)
            using (responseMessage)
            {
                // Log device → AlpacaSpy
                LogDeviceResponse(responseMessage, responseBytes, device);

                // Forward response to the original client
                context.Response.StatusCode = (int)responseMessage.StatusCode;

                foreach (var header in responseMessage.Headers)
                {
                    if (!HopByHopHeaders.Contains(header.Key))
                        context.Response.Headers[header.Key] = header.Value.ToArray();
                }
                foreach (var header in responseMessage.Content.Headers)
                {
                    if (!HopByHopHeaders.Contains(header.Key))
                        context.Response.Headers[header.Key] = header.Value.ToArray();
                }


                await context.Response.Body.WriteAsync(responseBytes);
            }
        }

        private static async Task<byte[]> ReadRequestBodyAsync(HttpContext context)
        {
            if (!context.Request.Method.Equals("PUT", StringComparison.OrdinalIgnoreCase))
                return [];

            context.Request.EnableBuffering();
            using var ms = new MemoryStream();
            await context.Request.Body.CopyToAsync(ms);
            context.Request.Body.Position = 0;
            return ms.ToArray();
        }

        private static HttpRequestMessage BuildForwardRequest(HttpContext context, string targetUrl, byte[] bodyBytes)
        {
            var request = new HttpRequestMessage
            {
                Method = new HttpMethod(context.Request.Method),
                RequestUri = new Uri(targetUrl)
            };

            // Collect headers rejected by HttpRequestMessage.Headers (i.e. content headers) so they
            // can be applied to request.Content after the body is assigned — not before (fix: was null).
            var deferredContentHeaders = new List<(string Key, string[] Values)>();
            foreach (var header in context.Request.Headers)
            {
                if (HopByHopHeaders.Contains(header.Key)) continue;
                if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
                    deferredContentHeaders.Add((header.Key, header.Value.ToArray()));
            }

            if (bodyBytes.Length > 0)
            {
                request.Content = new ByteArrayContent(bodyBytes);
                foreach (var (key, values) in deferredContentHeaders)
                    request.Content.Headers.TryAddWithoutValidation(key, values);
            }

            return request;
        }

        private void LogClientRequest(
            HttpContext context, ConfiguredDevice device, byte[] bodyBytes, string method)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"──► {context.Request.Method} /api/v1/{device.DeviceType.ToString().ToLower()}/{device.ProxyDeviceNumber}/{method}{context.Request.QueryString}  [{device.Name}]");

            if (device.LogClientHeaders)
            {
                sb.AppendLine("  CLIENT HEADERS:");
                foreach (var h in context.Request.Headers)
                    sb.AppendLine($"    {h.Key}: {h.Value}");
            }

            if (device.LogClientQueryParams && context.Request.QueryString.HasValue)
            {
                sb.AppendLine("  QUERY PARAMETERS:");
                foreach (var q in context.Request.Query)
                    sb.AppendLine($"    {q.Key} = {q.Value}");
            }

            if (device.LogClientBody && bodyBytes.Length > 0)
            {
                sb.AppendLine("  BODY PARAMETERS:");
                foreach (var part in Encoding.UTF8.GetString(bodyBytes).Split('&'))
                    sb.AppendLine($"    {part}");
            }

            WriteToLogs(device, sb.ToString().TrimEnd());
        }

        private void LogDeviceResponse(HttpResponseMessage response, byte[] responseBytes, ConfiguredDevice device)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"◄── {(int)response.StatusCode} {response.ReasonPhrase}  [{device.Name}]");

            if (device.LogDeviceHeaders)
            {
                sb.AppendLine("  DEVICE RESPONSE HEADERS:");
                foreach (var h in response.Headers)
                    sb.AppendLine($"    {h.Key}: {string.Join(", ", h.Value)}");
                foreach (var h in response.Content.Headers)
                    sb.AppendLine($"    {h.Key}: {string.Join(", ", h.Value)}");
            }

            string? contentType = response.Content.Headers.ContentType?.ToString().ToLowerInvariant();

            if (device.LogDeviceJson && responseBytes.Length > 0)
            {
                switch (contentType)
                {
                    case "application/json":
                        {
                            string text = Encoding.UTF8.GetString(responseBytes);
                            sb.AppendLine($"  DEVICE RESPONSE JSON: {text[..Math.Min(150, text.Length)]}");
                            break;
                        }
                    case "application/imagebytes":
                        {
                            string text = Encoding.ASCII.GetString(responseBytes);
                            sb.AppendLine($"  DEVICE RESPONSE IMAGEBYTES: {text[..Math.Min(50, text.Length)]}");
                            break;
                        }
                    default:
                        {
                            string text = Encoding.UTF8.GetString(responseBytes);
                            sb.AppendLine($"  DEVICE RESPONSE: {text[..Math.Min(150, text.Length)]}");
                            break;
                        }
                }
            }

            if (device.LogJsonParameters && responseBytes.Length > 0 && contentType != "application/imagebytes")
            {
                if (!device.LogDeviceJson && responseBytes.Length > 0)
                {
                    sb.AppendLine($"  DEVICE RESPONSE:");
                }
                try
                {
                    using var doc = JsonDocument.Parse(responseBytes);
                    foreach (var prop in doc.RootElement.EnumerateObject())
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

            if (state.DeviceLoggers.TryGetValue(device.UniqueId, out var traceLogger))
                traceLogger.LogMessage("Proxy", message);
        }

        private async Task ReturnAlpacaErrorAsync(
            HttpContext context, int errorNumber, string errorMessage, int statusCode = 500)
        {
            // Parse ClientTransactionID from query string so the client can correlate errors
            int clientTxId = 0;
            if (context.Request.Query.TryGetValue("ClientTransactionID", out var ctStr))
                int.TryParse(ctStr, out clientTxId);

            var error = new
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
