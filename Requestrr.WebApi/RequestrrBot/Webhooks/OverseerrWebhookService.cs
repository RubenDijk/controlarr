using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Requestrr.WebApi.Controllers.Webhooks;
using Requestrr.WebApi.RequestrrBot.ChatClients.Discord;

namespace Requestrr.WebApi.RequestrrBot.Webhooks
{
    public class OverseerrWebhookService
    {
        private readonly ILogger<OverseerrWebhookService> _logger;
        private readonly DiscordSettingsProvider _discordSettingsProvider;
        private readonly OverseerrModerationService _moderationService;
        private DiscordClient _discordClient;

        // Store pending requests with their message IDs
        private readonly ConcurrentDictionary<string, PendingModerationRequest> _pendingRequests = 
            new ConcurrentDictionary<string, PendingModerationRequest>();

        public OverseerrWebhookService(
            ILogger<OverseerrWebhookService> logger,
            DiscordSettingsProvider discordSettingsProvider,
            OverseerrModerationService moderationService)
        {
            _logger = logger;
            _discordSettingsProvider = discordSettingsProvider;
            _moderationService = moderationService;
        }

        public void SetDiscordClient(DiscordClient client)
        {
            _discordClient = client;
        }

        public async Task HandlePendingRequestAsync(OverseerrWebhookPayload payload)
        {
            try
            {
                if (_discordClient == null)
                {
                    _logger.LogWarning("Discord client not initialized");
                    return;
                }

                var settings = _discordSettingsProvider.Provide();
                
                if (settings.ModeratorChannels == null || !settings.ModeratorChannels.Any())
                {
                    _logger.LogWarning("No moderator channels configured");
                    return;
                }

                // Create embed for the pending request
                var embed = CreatePendingRequestEmbed(payload);

                // Create buttons
                var approveButton = new DiscordButtonComponent(
                    ButtonStyle.Success,
                    $"overseerr_approve_{payload.Request.RequestId}",
                    "✅ Approve"
                );

                var declineButton = new DiscordButtonComponent(
                    ButtonStyle.Danger,
                    $"overseerr_decline_{payload.Request.RequestId}",
                    "❌ Decline"
                );

                var builder = new DiscordMessageBuilder()
                    .AddEmbed(embed)
                    .AddComponents(approveButton, declineButton);

                // Send message to all configured moderator channels
                foreach (var channelIdStr in settings.ModeratorChannels)
                {
                    try
                    {
                        if (ulong.TryParse(channelIdStr, out var channelId))
                        {
                            var channel = await _discordClient.GetChannelAsync(channelId);
                            var message = await channel.SendMessageAsync(builder);

                            // Store the pending request
                            var pendingRequest = new PendingModerationRequest
                            {
                                RequestId = payload.Request.RequestId,
                                MessageId = message.Id,
                                ChannelId = channelId,
                                MediaType = payload.Media.MediaType,
                                TmdbId = payload.Media.TmdbId,
                                Subject = payload.Subject,
                                RequestedBy = payload.Request.RequestedByUsername,
                                RequestedByDiscordId = payload.Request.RequestedByDiscordId
                            };

                            _pendingRequests.TryAdd($"{message.Id}", pendingRequest);
                            
                            _logger.LogInformation($"Posted moderation request to channel {channelId} for request {payload.Request.RequestId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to send message to channel {channelIdStr}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling pending request");
                throw;
            }
        }

        public async Task HandleButtonInteractionAsync(DiscordClient client, DSharpPlus.EventArgs.ComponentInteractionCreateEventArgs e)
        {
            try
            {
                var customId = e.Id;
                
                if (!customId.StartsWith("overseerr_"))
                    return;

                await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                var parts = customId.Split('_');
                if (parts.Length != 3)
                    return;

                var action = parts[1]; // "approve" or "decline"
                var requestId = int.Parse(parts[2]);

                // Get the pending request
                if (!_pendingRequests.TryGetValue($"{e.Message.Id}", out var pendingRequest))
                {
                    await e.Interaction.CreateFollowupMessageAsync(
                        new DiscordFollowupMessageBuilder()
                            .WithContent("⚠️ Could not find the pending request information.")
                            .AsEphemeral(true)
                    );
                    return;
                }

                // Check if user has permission (has moderator role or is admin)
                var member = e.User as DiscordMember;
                if (member == null || !HasModeratorPermission(member))
                {
                    await e.Interaction.CreateFollowupMessageAsync(
                        new DiscordFollowupMessageBuilder()
                            .WithContent("⚠️ You don't have permission to moderate requests.")
                            .AsEphemeral(true)
                    );
                    return;
                }

                // Process the action
                bool success;
                string resultMessage;

                if (action == "approve")
                {
                    success = await _moderationService.ApproveRequestAsync(requestId);
                    resultMessage = success 
                        ? $"✅ Request approved by {e.User.Mention}" 
                        : "❌ Failed to approve request";
                }
                else // decline
                {
                    success = await _moderationService.DeclineRequestAsync(requestId);
                    resultMessage = success 
                        ? $"❌ Request declined by {e.User.Mention}" 
                        : "❌ Failed to decline request";
                }

                // Update the message
                var originalEmbed = e.Message.Embeds.FirstOrDefault();
                var updatedEmbed = new DiscordEmbedBuilder(originalEmbed)
                    .WithColor(success ? (action == "approve" ? DiscordColor.Green : DiscordColor.Red) : DiscordColor.Orange)
                    .WithFooter($"{resultMessage} • {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                    .Build();

                await e.Interaction.EditOriginalResponseAsync(
                    new DiscordWebhookBuilder()
                        .AddEmbed(updatedEmbed)
                );

                // Remove buttons by editing the message
                await e.Message.ModifyAsync(msg =>
                {
                    msg.AddEmbed(updatedEmbed);
                    msg.ClearComponents();
                });

                // Clean up
                _pendingRequests.TryRemove($"{e.Message.Id}", out _);

                _logger.LogInformation($"Request {requestId} {action}d by {e.User.Username}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling button interaction");
                
                try
                {
                    await e.Interaction.CreateFollowupMessageAsync(
                        new DiscordFollowupMessageBuilder()
                            .WithContent("❌ An error occurred while processing your request.")
                            .AsEphemeral(true)
                    );
                }
                catch { }
            }
        }

        private DiscordEmbed CreatePendingRequestEmbed(OverseerrWebhookPayload payload)
        {
            var embedBuilder = new DiscordEmbedBuilder()
                .WithTitle("🔔 New Pending Request")
                .WithDescription($"**{payload.Subject}**\n\n{payload.Message}")
                .WithColor(DiscordColor.Yellow)
                .WithTimestamp(DateTime.Now);

            if (!string.IsNullOrEmpty(payload.Image))
            {
                embedBuilder.WithThumbnail(payload.Image);
            }

            embedBuilder.AddField("Requested By", payload.Request.RequestedByUsername ?? "Unknown", true);
            embedBuilder.AddField("Media Type", payload.Media.MediaType ?? "Unknown", true);
            embedBuilder.AddField("Request ID", payload.Request.RequestId.ToString(), true);

            if (payload.Extra != null && payload.Extra.Any())
            {
                foreach (var extra in payload.Extra)
                {
                    embedBuilder.AddField(extra.Name, extra.Value, true);
                }
            }

            return embedBuilder.Build();
        }

        private bool HasModeratorPermission(DiscordMember member)
        {
            // Check if user is administrator
            if (member.Permissions.HasPermission(Permissions.Administrator))
                return true;

            // Check if user has manage messages permission (common moderator permission)
            if (member.Permissions.HasPermission(Permissions.ManageMessages))
                return true;

            // You can add additional role checks here if needed
            return false;
        }

        public class PendingModerationRequest
        {
            public int RequestId { get; set; }
            public ulong MessageId { get; set; }
            public ulong ChannelId { get; set; }
            public string MediaType { get; set; }
            public int TmdbId { get; set; }
            public string Subject { get; set; }
            public string RequestedBy { get; set; }
            public string RequestedByDiscordId { get; set; }
        }
    }
}
