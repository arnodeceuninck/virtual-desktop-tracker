using FluentAssertions;
using VirtualDesktopHelper.Services;
using Xunit;
using Xunit.Abstractions;

namespace VirtualDesktopHelper.Tests.Services
{
    /// <summary>
    /// Comprehensive unit tests for CurlRequestParser using xUnit and FluentAssertions.
    /// Tests various curl request formats and edge cases.
    /// </summary>
    public class CurlRequestParserTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly CurlRequestParser _parser;

        public CurlRequestParserTests(ITestOutputHelper output)
        {
            _output = output;
            _parser = new CurlRequestParser();
            _output.WriteLine("Setting up CurlRequestParser tests");
        }

        public void Dispose()
        {
            _output.WriteLine("Cleaning up CurlRequestParser tests");
        }

        #region Valid Curl Request Tests

        [Fact]
        [Trait("Category", "Parser")]
        public void ParseCurlRequest_ShouldExtractAllFields_FromValidWindowsCurlRequest()
        {
            // Arrange - Based on the provided example but with sanitized data
            var curlRequest = @"curl ^""https://app.timelyapp.com/123456/hours^"" ^
  -H ^""accept: application/json^"" ^
  -H ^""accept-language: en-US,en;q=0.9,nl;q=0.8^"" ^
  -H ^""cache-control: no-cache^"" ^
  -H ^""content-type: application/json^"" ^
  -b ^""_ga=GA1.1.123456789.1696240092; time_format=24; _ga_TEST=GS1.1.1724425674.4.1.1724425678.56.0.0; revision=current; _memory_session=test_session_id; timely_analytics_session=1755979421438^"" ^
  -H ^""origin: https://app.timelyapp.com^"" ^
  -H ^""tl-socket-id: 231807.1502190^"" ^
  -H ^""x-csrf-token: test_csrf_token_123^"" ^
  --data-raw ^""{^\""event^\"":{^\""day^\"":\""2025-08-23\"",^\""project_id^\"":9999999,^\""user_id^\"":1234567,^\""from^\"":\""2025-08-23T23:12:00.000+02:00\"",^\""to^\"":\""2025-08-23T23:28:00.000+02:00\""}}\""";

            // Act
            var result = _parser.ParseCurlRequest(curlRequest);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.ErrorMessage.Should().BeEmpty();
            
            result.WorkspaceId.Should().Be("123456");
            result.ProjectId.Should().Be(9999999);
            result.UserId.Should().Be(1234567);
            result.CsrfToken.Should().Be("test_csrf_token_123");
            result.SocketId.Should().Be("231807.1502190");
            result.CookieString.Should().Contain("_ga=GA1.1.123456789.1696240092");
            result.ApiBaseUrl.Should().Be("https://app.timelyapp.com");

            _output.WriteLine($"Successfully parsed Windows curl request for workspace: {result.WorkspaceId}");
        }

        [Fact]
        [Trait("Category", "Parser")]
        public void ParseCurlRequest_ShouldExtractAllFields_FromUnixCurlRequest()
        {
            // Arrange - Unix-style curl request (no Windows line continuation)
            var curlRequest = @"curl ""https://app.timelyapp.com/789012/calendar"" \
  -H ""accept: application/json"" \
  -H ""x-csrf-token: unix_csrf_token_456"" \
  -H ""tl-socket-id: 123.456"" \
  -b ""session_id=unix_session; user_pref=test"" \
  --data-raw '{""event"":{""project_id"":5555555,""user_id"":7890123}}'";

            // Act
            var result = _parser.ParseCurlRequest(curlRequest);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            
            result.WorkspaceId.Should().Be("789012");
            result.ProjectId.Should().Be(5555555);
            result.UserId.Should().Be(7890123);
            result.CsrfToken.Should().Be("unix_csrf_token_456");
            result.SocketId.Should().Be("123.456");
            result.CookieString.Should().Be("session_id=unix_session; user_pref=test");

            _output.WriteLine($"Successfully parsed Unix curl request for workspace: {result.WorkspaceId}");
        }

        [Fact]
        [Trait("Category", "Parser")]
        public void ParseCurlRequest_ShouldExtractCookiesFromHeader_WhenNoBFlag()
        {
            // Arrange - Curl request with cookies in header instead of -b flag
            var curlRequest = @"curl ""https://app.timelyapp.com/111111/hours"" \
  -H ""Cookie: auth_token=header_cookie_test; preferences=dark_mode"" \
  -H ""x-csrf-token: header_csrf_token"" \
  -H ""tl-socket-id: 999.888"" \
  --data-raw '{""event"":{""project_id"":1111111,""user_id"":2222222}}'";

            // Act
            var result = _parser.ParseCurlRequest(curlRequest);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            
            result.CookieString.Should().Be("auth_token=header_cookie_test; preferences=dark_mode");
            result.WorkspaceId.Should().Be("111111");

            _output.WriteLine("Successfully parsed cookies from Cookie header");
        }

        #endregion

        #region Edge Cases and Error Handling

        [Theory]
        [Trait("Category", "EdgeCase")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ParseCurlRequest_ShouldReturnInvalid_ForEmptyInput(string input)
        {
            // Act
            var result = _parser.ParseCurlRequest(input);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("Curl request is empty.");

            _output.WriteLine($"Correctly handled empty input: '{input}'");
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        public void ParseCurlRequest_ShouldReturnInvalid_ForInvalidUrl()
        {
            // Arrange
            var curlRequest = @"curl ""not-a-valid-url"" -H ""x-csrf-token: test""";

            // Act
            var result = _parser.ParseCurlRequest(curlRequest);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Missing required fields");

            _output.WriteLine("Correctly handled invalid URL");
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        public void ParseCurlRequest_ShouldReturnInvalid_ForMalformedJson()
        {
            // Arrange - Actually malformed JSON that will cause parsing to fail
            var curlRequest = @"curl ""https://app.timelyapp.com/123456/hours"" \
  -H ""x-csrf-token: test_token"" \
  -H ""tl-socket-id: 123.456"" \
  -b ""test=cookie"" \
  --data-raw '{""event"":{""project_id"":123,""user_id"":456,""completely""}}}'";

            // Act
            var result = _parser.ParseCurlRequest(curlRequest);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse(); // Should be invalid due to missing Project/User IDs after failed JSON parsing
            result.ProjectId.Should().Be(0); // Should default to 0 when JSON parsing fails
            result.UserId.Should().Be(0); // Should default to 0 when JSON parsing fails
            result.ErrorMessage.Should().Contain("Missing required fields");

            _output.WriteLine("Correctly handled JSON with syntax that causes parsing to fail");
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        public void ParseCurlRequest_ShouldReturnInvalid_WhenMissingRequiredFields()
        {
            // Arrange - Missing CSRF token and socket ID
            var curlRequest = @"curl ""https://app.timelyapp.com/123456/hours"" \
  --data-raw '{""event"":{""project_id"":123,""user_id"":456}}'";

            // Act
            var result = _parser.ParseCurlRequest(curlRequest);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Missing required fields");
            result.ErrorMessage.Should().Contain("CSRF Token");
            result.ErrorMessage.Should().Contain("Cookie String");

            _output.WriteLine($"Correctly identified missing fields: {result.ErrorMessage}");
        }

        [Theory]
        [Trait("Category", "EdgeCase")]
        [InlineData("https://app.timelyapp.com/999888/calendar", "999888")]
        [InlineData("https://app.timelyapp.com/1/reports", "1")]
        [InlineData("https://app.timelyapp.com/1234567890/dashboard", "1234567890")]
        public void ParseCurlRequest_ShouldExtractWorkspaceId_FromVariousUrls(string url, string expectedWorkspaceId)
        {
            // Arrange
            var curlRequest = $@"curl ""{url}"" \
  -H ""x-csrf-token: test_token"" \
  -H ""tl-socket-id: 123.456"" \
  -b ""test=cookie"" \
  --data-raw '{{""event"":{{""project_id"":123,""user_id"":456}}}}'";

            // Act
            var result = _parser.ParseCurlRequest(curlRequest);

            // Assert
            result.Should().NotBeNull();
            result.WorkspaceId.Should().Be(expectedWorkspaceId);

            _output.WriteLine($"Extracted workspace ID '{expectedWorkspaceId}' from URL: {url}");
        }

        #endregion

        #region JSON Parsing Tests

        [Fact]
        [Trait("Category", "JsonParsing")]
        public void ParseCurlRequest_ShouldExtractProjectAndUserId_FromComplexJson()
        {
            // Arrange
            var curlRequest = @"curl ""https://app.timelyapp.com/123456/hours"" \
  -H ""x-csrf-token: test_token"" \
  -H ""tl-socket-id: 123.456"" \
  -b ""test=cookie"" \
  --data-raw '{
    ""event"": {
      ""day"": ""2025-08-23"",
      ""note"": ""Test note"",
      ""project_id"": 8888888,
      ""user_id"": 9999999,
      ""from"": ""2025-08-23T10:00:00.000+02:00"",
      ""to"": ""2025-08-23T11:00:00.000+02:00"",
      ""hours"": 1,
      ""minutes"": 0,
      ""billable"": true
    }
  }'";

            // Act
            var result = _parser.ParseCurlRequest(curlRequest);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.ProjectId.Should().Be(8888888);
            result.UserId.Should().Be(9999999);

            _output.WriteLine($"Extracted Project ID: {result.ProjectId}, User ID: {result.UserId}");
        }

        [Fact]
        [Trait("Category", "JsonParsing")]
        public void ParseCurlRequest_ShouldHandleEscapedJson_FromWindowsCurl()
        {
            // Arrange - Windows-style escaped JSON
            var curlRequest = @"curl ""https://app.timelyapp.com/123456/hours"" \
  -H ""x-csrf-token: test_token"" \
  -H ""tl-socket-id: 123.456"" \
  -b ""test=cookie"" \
  --data-raw ""{\\""event\\"":{\\""project_id\\\"":7777777,\\""user_id\\\"":8888888}}""";

            // Act
            var result = _parser.ParseCurlRequest(curlRequest);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.ProjectId.Should().Be(7777777);
            result.UserId.Should().Be(8888888);

            _output.WriteLine("Successfully parsed escaped JSON from Windows curl");
        }

        [Fact]
        [Trait("Category", "JsonParsing")]
        public void ParseCurlRequest_ShouldReturnZeroIds_WhenJsonMissingFields()
        {
            // Arrange - JSON without project_id and user_id
            var curlRequest = @"curl ""https://app.timelyapp.com/123456/hours"" \
  -H ""x-csrf-token: test_token"" \
  -H ""tl-socket-id: 123.456"" \
  -b ""test=cookie"" \
  --data-raw '{""event"":{""day"":""2025-08-23"",""note"":""test""}}'";

            // Act
            var result = _parser.ParseCurlRequest(curlRequest);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse(); // Should be invalid due to missing required IDs
            result.ProjectId.Should().Be(0);
            result.UserId.Should().Be(0);
            result.ErrorMessage.Should().Contain("Missing required fields");

            _output.WriteLine("Correctly handled missing project_id and user_id in JSON");
        }

        #endregion

        #region Header Parsing Tests

        [Theory]
        [Trait("Category", "HeaderParsing")]
        [InlineData(@"-H ""x-csrf-token: simple_token""", "simple_token")]
        [InlineData(@"-H ""x-csrf-token:token_with_no_space""", "token_with_no_space")]
        [InlineData(@"-H ""X-CSRF-TOKEN: UPPERCASE_HEADER""", "UPPERCASE_HEADER")]
        [InlineData(@"-H 'x-csrf-token: single_quotes'", "single_quotes")]
        public void ParseCurlRequest_ShouldExtractCsrfToken_FromVariousFormats(string headerFormat, string expectedToken)
        {
            // Arrange
            var curlRequest = $@"curl ""https://app.timelyapp.com/123456/hours"" \
  {headerFormat} \
  -H ""tl-socket-id: 123.456"" \
  -b ""test=cookie"" \
  --data-raw '{{""event"":{{""project_id"":123,""user_id"":456}}}}'";

            // Act
            var result = _parser.ParseCurlRequest(curlRequest);

            // Assert
            result.Should().NotBeNull();
            result.CsrfToken.Should().Be(expectedToken);

            _output.WriteLine($"Extracted CSRF token '{expectedToken}' from format: {headerFormat}");
        }

        [Theory]
        [Trait("Category", "HeaderParsing")]
        [InlineData(@"-H ""tl-socket-id: 12345.67890""", "12345.67890")]
        [InlineData(@"-H ""TL-SOCKET-ID: UPPERCASE_SOCKET""", "UPPERCASE_SOCKET")]
        [InlineData(@"-H 'tl-socket-id: single_quote_socket'", "single_quote_socket")]
        public void ParseCurlRequest_ShouldExtractSocketId_FromVariousFormats(string headerFormat, string expectedSocketId)
        {
            // Arrange
            var curlRequest = $@"curl ""https://app.timelyapp.com/123456/hours"" \
  -H ""x-csrf-token: test_token"" \
  {headerFormat} \
  -b ""test=cookie"" \
  --data-raw '{{""event"":{{""project_id"":123,""user_id"":456}}}}'";

            // Act
            var result = _parser.ParseCurlRequest(curlRequest);

            // Assert
            result.Should().NotBeNull();
            result.SocketId.Should().Be(expectedSocketId);

            _output.WriteLine($"Extracted socket ID '{expectedSocketId}' from format: {headerFormat}");
        }

        #endregion

        #region URL and Base URL Tests

        [Theory]
        [Trait("Category", "UrlParsing")]
        [InlineData("https://app.timelyapp.com/123/test", "https://app.timelyapp.com")]
        [InlineData("http://app.timelyapp.com/456/test", "http://app.timelyapp.com")]
        [InlineData("https://custom.domain.com/789/test", "https://custom.domain.com")]
        public void ParseCurlRequest_ShouldExtractCorrectBaseUrl(string fullUrl, string expectedBaseUrl)
        {
            // Arrange
            var curlRequest = $@"curl ""{fullUrl}"" \
  -H ""x-csrf-token: test_token"" \
  -H ""tl-socket-id: 123.456"" \
  -b ""test=cookie"" \
  --data-raw '{{""event"":{{""project_id"":123,""user_id"":456}}}}'";

            // Act
            var result = _parser.ParseCurlRequest(curlRequest);

            // Assert
            result.Should().NotBeNull();
            result.ApiBaseUrl.Should().Be(expectedBaseUrl);

            _output.WriteLine($"Extracted base URL '{expectedBaseUrl}' from: {fullUrl}");
        }

        [Fact]
        [Trait("Category", "UrlParsing")]
        public void ParseCurlRequest_ShouldUseDefaultBaseUrl_ForInvalidUrl()
        {
            // Arrange
            var curlRequest = @"curl ""invalid-url-format"" \
  -H ""x-csrf-token: test_token"" \
  -H ""tl-socket-id: 123.456"" \
  -b ""test=cookie"" \
  --data-raw '{""event"":{""project_id"":123,""user_id"":456}}'";

            // Act
            var result = _parser.ParseCurlRequest(curlRequest);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse(); // Should be invalid due to missing workspace ID
            result.ApiBaseUrl.Should().Be(""); // Empty when URL extraction fails

            _output.WriteLine("Handled invalid URL format correctly");
        }

        #endregion

        #region Performance Tests

        [Fact]
        [Trait("Category", "Performance")]
        public void ParseCurlRequest_ShouldPerformWell_WithLargeCurlRequest()
        {
            // Arrange - Create a large curl request with many headers
            var largeHeaders = string.Join(" \\\n", 
                Enumerable.Range(1, 50).Select(i => $@"  -H ""custom-header-{i}: value-{i}"""));
            
            var curlRequest = $@"curl ""https://app.timelyapp.com/123456/hours"" \
  -H ""x-csrf-token: test_token"" \
  -H ""tl-socket-id: 123.456"" \
  -b ""test=cookie"" \
  {largeHeaders} \
  --data-raw '{{""event"":{{""project_id"":123,""user_id"":456}}}}'";

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = _parser.ParseCurlRequest(curlRequest);
            stopwatch.Stop();

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(100); // Should complete within 100ms

            _output.WriteLine($"Parsed large curl request in {stopwatch.ElapsedMilliseconds}ms");
        }

        #endregion

        #region Real-world Scenarios

        [Fact]
        [Trait("Category", "RealWorld")]
        public void ParseCurlRequest_ShouldHandleCompleteRealWorldExample()
        {
            // Arrange - Comprehensive real-world example with all components (single-line JSON)
            var curlRequest = @"curl ""https://app.timelyapp.com/555666/hours"" \
  -H ""accept: application/json, text/plain, */*"" \
  -H ""accept-language: en-US,en;q=0.9"" \
  -H ""cache-control: no-cache"" \
  -H ""content-type: application/json"" \
  -H ""origin: https://app.timelyapp.com"" \
  -H ""pragma: no-cache"" \
  -H ""referer: https://app.timelyapp.com/555666/calendar/day?date=2025-08-23"" \
  -H ""sec-ch-ua: \""Chromium\"";v=\""128\"", \""Not;A=Brand\"";v=\""24\"", \""Microsoft Edge\"";v=\""128\"""" \
  -H ""sec-ch-ua-mobile: ?0"" \
  -H ""sec-ch-ua-platform: \""Windows\"""" \
  -H ""sec-fetch-dest: empty"" \
  -H ""sec-fetch-mode: cors"" \
  -H ""sec-fetch-site: same-origin"" \
  -H ""tl-socket-id: 98765.43210"" \
  -H ""user-agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"" \
  -H ""x-csrf-token: real_world_csrf_token_example"" \
  -b ""_ga=GA1.1.987654321.1696240092; session_id=real_session; user_preferences=timezone:UTC"" \
  --data-raw '{""event"":{""day"":""2025-08-23"",""note"":""Development work on parser"",""project_id"":1122334,""user_id"":9988776,""from"":""2025-08-23T14:30:00.000+02:00"",""to"":""2025-08-23T16:30:00.000+02:00"",""hours"":2,""minutes"":0,""billable"":true,""context"":{""interaction"":""Manual Entry"",""view_context"":""Calendar""}}}'";

            // Act
            var result = _parser.ParseCurlRequest(curlRequest);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.ErrorMessage.Should().BeEmpty();
            
            // Verify all extracted values
            result.WorkspaceId.Should().Be("555666");
            result.ProjectId.Should().Be(1122334);
            result.UserId.Should().Be(9988776);
            result.CsrfToken.Should().Be("real_world_csrf_token_example");
            result.SocketId.Should().Be("98765.43210");
            result.CookieString.Should().Contain("_ga=GA1.1.987654321.1696240092");
            result.CookieString.Should().Contain("session_id=real_session");
            result.ApiBaseUrl.Should().Be("https://app.timelyapp.com");

            _output.WriteLine("Successfully parsed complete real-world curl request");
            _output.WriteLine($"Workspace: {result.WorkspaceId}, Project: {result.ProjectId}, User: {result.UserId}");
        }

        #endregion

        #region Timezone Offset Tests

        [Theory]
        [Trait("Category", "TimezoneOffset")]
        [InlineData("+02:00")]
        [InlineData("-05:00")]
        [InlineData("+00:00")]
        [InlineData("+05:30")]
        public void ParseCurlRequest_ShouldExtractTimezoneOffset_FromTimestamp(string expectedOffset)
        {
            // Arrange
            var curlRequest = $@"curl ""https://app.timelyapp.com/123456/hours"" \
  -H ""x-csrf-token: test_token"" \
  -H ""tl-socket-id: 123.456"" \
  -b ""test=cookie"" \
  --data-raw '{{""event"":{{""from"":""2025-08-23T10:00:00.000{expectedOffset}"",""to"":""2025-08-23T11:00:00.000{expectedOffset}"",""project_id"":123,""user_id"":456}}}}'";

            // Act
            var result = _parser.ParseCurlRequest(curlRequest);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.TimezoneOffset.Should().Be(expectedOffset);

            _output.WriteLine($"Extracted timezone offset: {result.TimezoneOffset}");
        }

        [Fact]
        [Trait("Category", "TimezoneOffset")]
        public void ParseCurlRequest_ShouldHandleMissingTimezoneOffset()
        {
            // Arrange - curl request without timezone in timestamp
            var curlRequest = @"curl ""https://app.timelyapp.com/123456/hours"" \
  -H ""x-csrf-token: test_token"" \
  -H ""tl-socket-id: 123.456"" \
  -b ""test=cookie"" \
  --data-raw '{""event"":{""project_id"":123,""user_id"":456}}'";

            // Act
            var result = _parser.ParseCurlRequest(curlRequest);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.TimezoneOffset.Should().BeNull();

            _output.WriteLine("Handled missing timezone offset gracefully");
        }

        [Fact]
        [Trait("Category", "TimezoneOffset")]
        public void ParseCurlRequest_ShouldExtractTimezoneFromComplexRealWorldExample()
        {
            // Arrange - Based on the issue description with DST timezone
            var curlRequest = @"curl ""https://app.timelyapp.com/946869/hours"" \
  -H ""accept: application/json"" \
  -H ""x-csrf-token: test_token"" \
  -H ""tl-socket-id: 123.456"" \
  -b ""test=cookie"" \
  --data-raw '{""event"":{""day"":""2025-08-23"",""from"":""2025-08-23T09:30:00.000+02:00"",""to"":""2025-08-23T10:30:00.000+02:00"",""project_id"":123,""user_id"":456}}'";

            // Act
            var result = _parser.ParseCurlRequest(curlRequest);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.TimezoneOffset.Should().Be("+02:00");
            result.WorkspaceId.Should().Be("946869");

            _output.WriteLine($"Extracted timezone from real-world example: {result.TimezoneOffset}");
        }

        #endregion
    }
}
