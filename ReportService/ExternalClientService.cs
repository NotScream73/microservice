using Newtonsoft.Json;
using System.Text;

namespace ReportService
{
    public class ExternalClientService
    {
        private readonly HttpClient _httpClient;

        public ExternalClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> SendRequestAsync(HttpRequest context, string url, HttpMethod method, object? body, HttpContent? content)
        {
            var request = new HttpRequestMessage(method, url);

            var traceId = context.Headers["X-Trace-Id"].ToString();

            if (!string.IsNullOrEmpty(traceId))
            {
                _httpClient.DefaultRequestHeaders.Remove("X-Trace-Id");
                _httpClient.DefaultRequestHeaders.Add("X-Trace-Id", traceId);
            }

            if (body != null)
            {
                var jsonBody = JsonConvert.SerializeObject(body);
                request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            }
            else if (content != null)
            {
                request.Content = content;
            }

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }

    }
}
