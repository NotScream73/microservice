using System.Text.Json.Serialization;

namespace ReportService.Models.DTO
{
    public class ValidationTokenResult
    {
        [JsonPropertyName("isValid")]
        public bool IsValid { get; set; }
        [JsonPropertyName("userId")]
        public int? UserId { get; set; }
        [JsonPropertyName("roles")]
        public string[]? Roles { get; set; }
    }
}
