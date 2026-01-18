using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Requestrr.WebApi.RequestrrBot.Webhooks;

namespace Requestrr.WebApi.Controllers.Webhooks
{
    [ApiController]
    [Route("api/webhooks/overseerr")]
    [AllowAnonymous]
    public class OverseerrWebhookController : ControllerBase
    {
        private readonly ILogger<OverseerrWebhookController> _logger;
        private readonly OverseerrWebhookService _webhookService;

        public OverseerrWebhookController(
            ILogger<OverseerrWebhookController> logger,
            OverseerrWebhookService webhookService)
        {
            _logger = logger;
            _webhookService = webhookService;
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook([FromBody] OverseerrWebhookPayload payload)
        {
            try
            {
                _logger.LogInformation($"Received Overseerr webhook: {payload?.NotificationType}");

                if (payload == null)
                {
                    _logger.LogWarning("Received null payload");
                    return BadRequest("Invalid payload");
                }

                // Only handle pending approval notifications
                if (payload.NotificationType == "MEDIA_PENDING")
                {
                    await _webhookService.HandlePendingRequestAsync(payload);
                    return Ok(new { message = "Webhook processed successfully" });
                }

                _logger.LogInformation($"Ignoring webhook type: {payload.NotificationType}");
                return Ok(new { message = "Webhook type not handled" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Overseerr webhook");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }

    public class OverseerrWebhookPayload
    {
        [JsonProperty("notification_type")]
        public string NotificationType { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("media")]
        public OverseerrMediaInfo Media { get; set; }

        [JsonProperty("request")]
        public OverseerrRequestInfo Request { get; set; }

        [JsonProperty("extra")]
        public OverseerrExtraInfo[] Extra { get; set; }
    }

    public class OverseerrMediaInfo
    {
        [JsonProperty("media_type")]
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

    public class OverseerrRequestInfo
    {
        [JsonProperty("request_id")]
        public int RequestId { get; set; }

        [JsonProperty("requestedBy_email")]
        public string RequestedByEmail { get; set; }

        [JsonProperty("requestedBy_username")]
        public string RequestedByUsername { get; set; }

        [JsonProperty("requestedBy_avatar")]
        public string RequestedByAvatar { get; set; }

        [JsonProperty("requestedBy_settings_discordId")]
        public string RequestedByDiscordId { get; set; }
    }

    public class OverseerrExtraInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
