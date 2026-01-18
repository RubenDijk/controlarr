# Overseerr Webhook Moderation - Implementation Summary

## Overzicht

Deze implementatie voegt Overseerr webhook integratie toe aan Requestrr, waarmee moderators pending requests kunnen goedkeuren of afwijzen via Discord-knoppen.

## Workflow

1. **Overseerr** → Stuurt webhook bij pending request → **Requestrr**
2. **Requestrr** → Post bericht met knoppen → **Discord moderator kanaal**
3. **Moderator** → Klikt op Approve/Decline knop → **Discord**
4. **Requestrr** → Roept Overseerr API aan → **Overseerr**
5. **Requestrr** → Update Discord bericht met resultaat → **Discord**

## Nieuwe Bestanden

### 1. Controllers
- **`Requestrr.WebApi/Controllers/Webhooks/OverseerrWebhookController.cs`**
  - REST API endpoint voor Overseerr webhooks
  - Route: `POST /api/webhooks/overseerr`
  - Accepteert `MEDIA_PENDING` notifications
  - Delegeert naar `OverseerrWebhookService`

### 2. Services
- **`Requestrr.WebApi/RequestrrBot/Webhooks/OverseerrWebhookService.cs`**
  - Verwerkt webhook payloads
  - Creëert Discord embeds met knoppen
  - Post berichten naar moderator kanalen
  - Handelt button interactions af
  - Valideert moderator permissions
  - Beheert pending request state

- **`Requestrr.WebApi/RequestrrBot/Webhooks/OverseerrModerationService.cs`**
  - Integreert met Overseerr API
  - `ApproveRequestAsync(requestId)` - Keurt request goed
  - `DeclineRequestAsync(requestId)` - Wijst request af
  - `GetRequestDetailsAsync(requestId)` - Haalt request details op

### 3. Documentatie
- **`OVERSEERR_WEBHOOK_SETUP.md`**
  - Complete setup guide in Nederlands
  - Configuratie instructies
  - Troubleshooting tips
  - API documentatie

- **`IMPLEMENTATION_SUMMARY.md`** (dit bestand)
  - Technische implementatie details
  - Overzicht van wijzigingen

## Gewijzigde Bestanden

### 1. Settings & Configuration
- **`Requestrr.WebApi/RequestrrBot/ChatClients/Discord/DiscordSettings.cs`**
  - Toegevoegd: `public string[] ModeratorChannels { get; set; }`
  - Updated: `Equals()` en `GetHashCode()` methoden

- **`Requestrr.WebApi/SettingsTemplate.json`**
  - Toegevoegd: `"ModeratorChannels": []` onder Discord settings

### 2. Core Bot Logic
- **`Requestrr.WebApi/RequestrrBot/ChatBot.cs`**
  - Toegevoegd: `using Requestrr.WebApi.RequestrrBot.Webhooks;`
  - Toegevoegd: `private OverseerrWebhookService _overseerrWebhookService;`
  - Constructor: Injecteert `OverseerrWebhookService`
  - `Connected()`: Initialiseert webhook service met Discord client
  - `DiscordComponentInteractionCreatedHandler()`: Handelt `overseerr_*` button interactions af

### 3. Dependency Injection
- **`Requestrr.WebApi/Startup.cs`**
  - Toegevoegd: `using Requestrr.WebApi.RequestrrBot.Webhooks;`
  - Geregistreerd: `services.AddSingleton<OverseerrModerationService>();`
  - Geregistreerd: `services.AddSingleton<OverseerrWebhookService>();`

## Technische Details

### Discord Button Interaction
- Button IDs: `overseerr_approve_{requestId}` en `overseerr_decline_{requestId}`
- Buttons worden verwijderd na actie
- Embed wordt bijgewerkt met resultaat en timestamp
- Ephemeral messages voor errors en permission denied

### Permission Checks
Moderators moeten één van de volgende hebben:
- `Permissions.Administrator`
- `Permissions.ManageMessages`

### State Management
- `ConcurrentDictionary<string, PendingModerationRequest>` voor pending requests
- Key: Message ID
- Automatische cleanup na actie

### Overseerr API Integration
- Base URL constructie met SSL/port/baseUrl support
- API Key authenticatie via `X-Api-Key` header
- Endpoints:
  - `POST /api/v1/request/{requestId}/approve`
  - `POST /api/v1/request/{requestId}/decline`
  - `GET /api/v1/request/{requestId}`

### Error Handling
- Try-catch blocks in alle async methoden
- Logging via `ILogger<T>`
- Graceful degradation bij failures
- User-friendly error messages in Discord

## Data Models

### OverseerrWebhookPayload
```csharp
- NotificationType: string
- Subject: string
- Message: string
- Image: string
- Media: OverseerrMediaInfo
- Request: OverseerrRequestInfo
- Extra: OverseerrExtraInfo[]
```

### PendingModerationRequest
```csharp
- RequestId: int
- MessageId: ulong
- ChannelId: ulong
- MediaType: string
- TmdbId: int
- Subject: string
- RequestedBy: string
- RequestedByDiscordId: string
```

## Configuratie Voorbeeld

```json
{
  "ChatClients": {
    "Discord": {
      "BotToken": "your-bot-token",
      "ClientId": "your-client-id",
      "MonitoredChannels": ["123456789"],
      "ModeratorChannels": ["987654321"],
      ...
    }
  },
  "DownloadClients": {
    "Overseerr": {
      "Hostname": "overseerr.example.com",
      "Port": 5055,
      "ApiKey": "your-api-key",
      "UseSSL": true,
      ...
    }
  }
}
```

## Testing Checklist

- [ ] Webhook endpoint bereikbaar vanaf Overseerr
- [ ] Discord bot heeft juiste permissions in moderator kanaal
- [ ] Moderator channels correct geconfigureerd
- [ ] Overseerr API key heeft approve/decline permissions
- [ ] Button interactions werken voor moderators
- [ ] Permission denied voor niet-moderators
- [ ] Error handling werkt correct
- [ ] Messages worden correct bijgewerkt
- [ ] Logging werkt correct

## Veiligheid Overwegingen

1. **Webhook Endpoint**: Publiekelijk toegankelijk (geen auth)
   - Overweeg webhook signing toe te voegen in toekomst
   
2. **Permission Checks**: Server-side validatie van moderator permissions
   
3. **API Key**: Opgeslagen in configuratie, niet in code
   
4. **Rate Limiting**: Niet geïmplementeerd (overweeg voor productie)

## Toekomstige Verbeteringen

1. **Webhook Authenticatie**
   - Signature verification
   - API key/token voor webhook endpoint

2. **Moderator Roles**
   - Configureerbare role IDs naast permissions
   - Per-kanaal role configuratie

3. **Decline Redenen**
   - Modal voor decline reden
   - Reden opslaan in Overseerr

4. **Notificaties**
   - DM naar requester bij approve/decline
   - Configureerbare notificatie templates

5. **Statistieken**
   - Dashboard met approve/decline stats
   - Per-moderator statistieken

6. **Audit Log**
   - Dedicated audit log kanaal
   - Gedetailleerde actie logging

7. **Bulk Actions**
   - Meerdere requests tegelijk modereren
   - Filters voor pending requests

## Dependencies

Geen nieuwe externe dependencies toegevoegd. Gebruikt bestaande:
- DSharpPlus (Discord library)
- Newtonsoft.Json (JSON serialization)
- Microsoft.Extensions.* (DI, Logging, HTTP)

## Compatibiliteit

- Requestrr versie: 2.1.3+
- Overseerr API: v1
- Discord API: v10 (via DSharpPlus)
- .NET: Compatible met bestaande project versie

## Build & Deploy

Geen speciale build stappen nodig:
1. Build project normaal
2. Update configuratie met ModeratorChannels
3. Configureer Overseerr webhook
4. Restart Requestrr

## Support & Troubleshooting

Zie `OVERSEERR_WEBHOOK_SETUP.md` voor:
- Setup instructies
- Troubleshooting guide
- FAQ
- Contact informatie
