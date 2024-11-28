using Newtonsoft.Json;
using ReportService.Exceptions;
using ReportService.Models.DTO;
using System.Net.Http.Headers;
using System.Text;

namespace ReportService.Services
{
    public class ExternalClientService
    {
        private readonly HttpClient _httpClient;

        public ExternalClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TResponse?> SendRequestAsync<TResponse>(string accessToken, string url, HttpMethod method, object? body)
        {
            var request = new HttpRequestMessage(method, url);

            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            if (body != null)
            {
                var jsonBody = JsonConvert.SerializeObject(body);
                request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            }

            var response = await _httpClient.SendAsync(request);

            switch ((int)response.StatusCode)
            {
                case int statusCode when statusCode >= 500 && statusCode <= 599:
                    throw new ServiceUnavailableException("Сервис не доступен.");
                case 404:
                    throw new NotFoundException(JsonConvert.DeserializeObject<NotFoundExceptionResponse>(await response.Content.ReadAsStringAsync()).Error);
                default:
                    response.EnsureSuccessStatusCode();
                    var responseBody = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<TResponse>(responseBody);
            }

        }

    }
}
