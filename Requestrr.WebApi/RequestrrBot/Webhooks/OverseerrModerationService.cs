using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Overseerr;

namespace Requestrr.WebApi.RequestrrBot.Webhooks
{
    public class OverseerrModerationService
    {
        private readonly ILogger<OverseerrModerationService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly OverseerrSettingsProvider _overseerrSettingsProvider;

        public OverseerrModerationService(
            ILogger<OverseerrModerationService> logger,
            IHttpClientFactory httpClientFactory,
            OverseerrSettingsProvider overseerrSettingsProvider)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _overseerrSettingsProvider = overseerrSettingsProvider;
        }

        public async Task<bool> ApproveRequestAsync(int requestId)
        {
            try
            {
                var settings = _overseerrSettingsProvider.Provide();
                var baseUrl = GetBaseUrl(settings);
                var url = $"{baseUrl}request/{requestId}/approve";

                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Add("X-Api-Key", settings.ApiKey);

                var response = await httpClient.PostAsync(url, null);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Successfully approved Overseerr request {requestId}");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to approve Overseerr request {requestId}. Status: {response.StatusCode}, Error: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error approving Overseerr request {requestId}");
                return false;
            }
        }

        public async Task<bool> DeclineRequestAsync(int requestId)
        {
            try
            {
                var settings = _overseerrSettingsProvider.Provide();
                var baseUrl = GetBaseUrl(settings);
                var url = $"{baseUrl}request/{requestId}/decline";

                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Add("X-Api-Key", settings.ApiKey);

                var response = await httpClient.PostAsync(url, null);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Successfully declined Overseerr request {requestId}");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to decline Overseerr request {requestId}. Status: {response.StatusCode}, Error: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error declining Overseerr request {requestId}");
                return false;
            }
        }

        public async Task<OverseerrRequestDetails> GetRequestDetailsAsync(int requestId)
        {
            try
            {
                var settings = _overseerrSettingsProvider.Provide();
                var baseUrl = GetBaseUrl(settings);
                var url = $"{baseUrl}request/{requestId}";

                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Add("X-Api-Key", settings.ApiKey);

                var response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var details = JsonConvert.DeserializeObject<OverseerrRequestDetails>(content);
                    return details;
                }
                else
                {
                    _logger.LogError($"Failed to get Overseerr request details for {requestId}. Status: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting Overseerr request details for {requestId}");
                return null;
            }
        }

        private string GetBaseUrl(OverseerrSettings settings)
        {
            var protocol = settings.UseSSL ? "https" : "http";
            var baseUrl = $"{protocol}://{settings.Hostname}:{settings.Port}/api/v1/";
            
            if (!string.IsNullOrWhiteSpace(settings.BaseUrl))
            {
                baseUrl = $"{protocol}://{settings.Hostname}:{settings.Port}{settings.BaseUrl}/api/v1/";
            }

            return baseUrl;
        }
    }

    public class OverseerrRequestDetails
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("media")]
        public OverseerrMediaDetails Media { get; set; }

        [JsonProperty("requestedBy")]
        public OverseerrUserDetails RequestedBy { get; set; }

        [JsonProperty("modifiedBy")]
        public OverseerrUserDetails ModifiedBy { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }

    public class OverseerrMediaDetails
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("mediaType")]
        public string MediaType { get; set; }

        [JsonProperty("tmdbId")]
        public int TmdbId { get; set; }

        [JsonProperty("tvdbId")]
        public int? TvdbId { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("status4k")]
        public int? Status4k { get; set; }
    }

    public class OverseerrUserDetails
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("plexUsername")]
        public string PlexUsername { get; set; }

        [JsonProperty("avatar")]
        public string Avatar { get; set; }
    }
}
