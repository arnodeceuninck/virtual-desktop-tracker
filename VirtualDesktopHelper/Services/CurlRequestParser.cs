using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace VirtualDesktopHelper.Services
{
    /// <summary>
    /// Service for parsing curl requests and extracting Timely API configuration.
    /// </summary>
    public class CurlRequestParser
    {
        /// <summary>
        /// Parsed configuration data from a curl request.
        /// </summary>
        public class ParsedCurlConfig
        {
            public string WorkspaceId { get; set; } = "";
            public long ProjectId { get; set; }
            public long UserId { get; set; }
            public string CsrfToken { get; set; } = "";
            public string SocketId { get; set; } = "";
            public string CookieString { get; set; } = "";
            public string ApiBaseUrl { get; set; } = "";
            public bool IsValid { get; set; }
            public string ErrorMessage { get; set; } = "";
        }

        /// <summary>
        /// Parses a curl request and extracts Timely configuration parameters.
        /// </summary>
        /// <param name="curlRequest">The curl request string</param>
        /// <returns>Parsed configuration data</returns>
        public ParsedCurlConfig ParseCurlRequest(string curlRequest)
        {
            var result = new ParsedCurlConfig();

            try
            {
                if (string.IsNullOrWhiteSpace(curlRequest))
                {
                    result.ErrorMessage = "Curl request is empty.";
                    return result;
                }

                // Clean up the curl request - remove line breaks and excess whitespace
                var cleanedCurl = CleanCurlRequest(curlRequest);

                // Extract URL and workspace ID
                ExtractUrlAndWorkspaceId(cleanedCurl, result);

                // Extract headers
                ExtractHeaders(cleanedCurl, result);

                // Extract data payload for project_id and user_id
                ExtractDataPayload(cleanedCurl, result);

                // Extract timezone offset from timestamps in the payload
                ExtractTimezoneOffset(cleanedCurl, result);

                // Validate the extracted data
                ValidateExtractedData(result);
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Error parsing curl request: {ex.Message}";
                result.IsValid = false;
            }

            return result;
        }

        /// <summary>
        /// Cleans the curl request by removing unnecessary characters and formatting.
        /// </summary>
        private string CleanCurlRequest(string curlRequest)
        {
            // Remove Windows line continuation characters (^)
            var cleaned = curlRequest.Replace("^", "");
            
            // Remove excessive whitespace and newlines
            cleaned = Regex.Replace(cleaned, @"\s+", " ");
            
            // Trim
            cleaned = cleaned.Trim();

            return cleaned;
        }

        /// <summary>
        /// Extracts URL and workspace ID from the curl request.
        /// </summary>
        private void ExtractUrlAndWorkspaceId(string curlRequest, ParsedCurlConfig result)
        {
            // Pattern to match the URL in curl request
            var urlPattern = @"curl\s+[""']?(https?://[^""'\s]+)[""']?";
            var urlMatch = Regex.Match(curlRequest, urlPattern);

            if (urlMatch.Success)
            {
                var fullUrl = urlMatch.Groups[1].Value;
                result.ApiBaseUrl = GetBaseUrl(fullUrl);

                // Extract workspace ID from URL pattern like: https://app.timelyapp.com/946869/hours
                var workspacePattern = @"app\.timelyapp\.com/(\d+)/";
                var workspaceMatch = Regex.Match(fullUrl, workspacePattern);
                if (workspaceMatch.Success)
                {
                    result.WorkspaceId = workspaceMatch.Groups[1].Value;
                }
            }
        }

        /// <summary>
        /// Extracts headers from the curl request.
        /// </summary>
        private void ExtractHeaders(string curlRequest, ParsedCurlConfig result)
        {
            // Extract x-csrf-token
            var csrfPattern = @"-H\s+[""']x-csrf-token:\s*([^""']+)[""']";
            var csrfMatch = Regex.Match(curlRequest, csrfPattern, RegexOptions.IgnoreCase);
            if (csrfMatch.Success)
            {
                result.CsrfToken = csrfMatch.Groups[1].Value.Trim();
            }

            // Extract tl-socket-id
            var socketPattern = @"-H\s+[""']tl-socket-id:\s*([^""']+)[""']";
            var socketMatch = Regex.Match(curlRequest, socketPattern, RegexOptions.IgnoreCase);
            if (socketMatch.Success)
            {
                result.SocketId = socketMatch.Groups[1].Value.Trim();
            }

            // Extract cookies (-b flag or cookie header)
            ExtractCookies(curlRequest, result);
        }

        /// <summary>
        /// Extracts cookies from the curl request.
        /// </summary>
        private void ExtractCookies(string curlRequest, ParsedCurlConfig result)
        {
            // Try to find -b flag first (preferred method)
            var cookieFlagPattern = @"-b\s+[""']([^""']+)[""']";
            var cookieFlagMatch = Regex.Match(curlRequest, cookieFlagPattern);
            if (cookieFlagMatch.Success)
            {
                result.CookieString = cookieFlagMatch.Groups[1].Value.Trim();
                return;
            }

            // Fallback to cookie header
            var cookieHeaderPattern = @"-H\s+[""']cookie:\s*([^""']+)[""']";
            var cookieHeaderMatch = Regex.Match(curlRequest, cookieHeaderPattern, RegexOptions.IgnoreCase);
            if (cookieHeaderMatch.Success)
            {
                result.CookieString = cookieHeaderMatch.Groups[1].Value.Trim();
            }
        }

        /// <summary>
        /// Extracts data payload and parses JSON to get project_id and user_id.
        /// </summary>
        private void ExtractDataPayload(string curlRequest, ParsedCurlConfig result)
        {
            // Extract the --data-raw content
            var dataPattern = @"--data-raw\s+[""'](.+?)[""'](?:\s|$)";
            var dataMatch = Regex.Match(curlRequest, dataPattern, RegexOptions.Singleline);
            
            if (dataMatch.Success)
            {
                var jsonData = dataMatch.Groups[1].Value;
                
                // Clean up the JSON - remove escape characters
                jsonData = jsonData.Replace("\\\"", "\"");
                jsonData = jsonData.Replace("\\", "");
                
                try
                {
                    using (JsonDocument doc = JsonDocument.Parse(jsonData))
                    {
                        var root = doc.RootElement;
                        
                        if (root.TryGetProperty("event", out var eventElement))
                        {
                            if (eventElement.TryGetProperty("project_id", out var projectIdElement))
                            {
                                result.ProjectId = projectIdElement.GetInt64();
                            }
                            
                            if (eventElement.TryGetProperty("user_id", out var userIdElement))
                            {
                                result.UserId = userIdElement.GetInt64();
                            }
                        }
                    }
                }
                catch (JsonException ex)
                {
                    result.ErrorMessage = $"Failed to parse JSON data: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Extracts timezone offset from the timestamp in the data payload.
        /// </summary>
        private void ExtractTimezoneOffset(string curlRequest, ParsedCurlConfig result)
        {
            // Look for timestamp patterns like "2025-08-23T23:12:00.000+02:00"
            var timezonePattern = @"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}([+-]\d{2}:\d{2})";
            var timezoneMatch = Regex.Match(curlRequest, timezonePattern);
            
            if (timezoneMatch.Success)
            {
                result.TimezoneOffset = timezoneMatch.Groups[1].Value;
            }
        }

        /// <summary>
        /// Validates that all required data was extracted successfully.
        /// </summary>
        private void ValidateExtractedData(ParsedCurlConfig result)
        {
            var missingFields = new List<string>();

            if (string.IsNullOrWhiteSpace(result.WorkspaceId))
                missingFields.Add("Workspace ID");
            
            if (result.ProjectId == 0)
                missingFields.Add("Project ID");
            
            if (result.UserId == 0)
                missingFields.Add("User ID");
            
            if (string.IsNullOrWhiteSpace(result.CsrfToken))
                missingFields.Add("CSRF Token");
            
            if (string.IsNullOrWhiteSpace(result.CookieString))
                missingFields.Add("Cookie String");

            if (missingFields.Any())
            {
                result.ErrorMessage = $"Missing required fields: {string.Join(", ", missingFields)}";
                result.IsValid = false;
            }
            else
            {
                result.IsValid = true;
            }
        }

        /// <summary>
        /// Extracts the base URL from a full URL.
        /// </summary>
        private string GetBaseUrl(string fullUrl)
        {
            try
            {
                var uri = new Uri(fullUrl);
                return $"{uri.Scheme}://{uri.Host}";
            }
            catch
            {
                return "https://app.timelyapp.com";
            }
        }
    }
}
