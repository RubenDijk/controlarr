# Overseerr Webhook Integration - Setup Guide

Deze feature voegt moderatie-functionaliteit toe aan Requestrr voor Overseerr pending requests.

## Functionaliteit

Wanneer er in Overseerr een pending request binnenkomt:
1. Overseerr stuurt een webhook naar Requestrr
2. Requestrr post een Discord-bericht in het geconfigureerde moderator-kanaal
3. Het bericht bevat informatie over de request en twee knoppen: **Approve** en **Decline**
4. Moderators kunnen op de knoppen klikken om de request goed te keuren of af te wijzen
5. Requestrr roept de Overseerr API aan om de actie uit te voeren
6. Het bericht wordt bijgewerkt met het resultaat (succes/fout)

## Configuratie

### 1. Moderator Kanaal Configureren in Requestrr

Voeg het moderator kanaal ID toe aan de Requestrr configuratie:

1. Open het Requestrr configuratiebestand: `config/SettingsFile.json`
2. Voeg onder `ChatClients.Discord` het volgende toe:
   ```json
   "ModeratorChannels": ["CHANNEL_ID_HIER"]
   ```
   
   Voorbeeld:
   ```json
   "ChatClients": {
     "Discord": {
       "BotToken": "your-bot-token",
       "ClientId": "your-client-id",
       "StatusMessage": "/help",
       "MonitoredChannels": ["123456789"],
       "ModeratorChannels": ["987654321"],
       ...
     }
   }
   ```

**Hoe vind je het Channel ID?**
1. Schakel Developer Mode in Discord in (User Settings > Advanced > Developer Mode)
2. Rechtermuisklik op het kanaal en selecteer "Copy ID"

### 2. Webhook Configureren in Overseerr

1. Log in op Overseerr
2. Ga naar **Settings** > **Notifications** > **Webhook**
3. Schakel de webhook in
4. Configureer de webhook URL:
   ```
   http://REQUESTRR_HOST:4545/api/webhooks/overseerr
   ```
   
   Vervang `REQUESTRR_HOST` met het IP-adres of hostname van je Requestrr server.
   
   Voorbeelden:
   - `http://localhost:4545/api/webhooks/overseerr`
   - `http://192.168.1.100:4545/api/webhooks/overseerr`
   - `http://requestrr.example.com:4545/api/webhooks/overseerr`

5. Selecteer de notification types:
   - ✅ **Media Pending** (vereist voor deze feature)
   
6. Sla de instellingen op

### 3. Discord Bot Permissions

Zorg ervoor dat de Discord bot de volgende permissions heeft in het moderator kanaal:
- **View Channel** - Om het kanaal te kunnen zien
- **Send Messages** - Om berichten te kunnen sturen
- **Embed Links** - Om embedded berichten te kunnen sturen
- **Use External Emojis** - Voor emoji's in knoppen

### 4. Moderator Permissions

Gebruikers die requests kunnen modereren moeten één van de volgende permissions hebben:
- **Administrator** - Volledige server admin rechten
- **Manage Messages** - Standaard moderator permission

## Gebruik

### Voor Moderators

1. Wanneer een nieuwe pending request binnenkomt, verschijnt er een bericht in het moderator kanaal
2. Het bericht bevat:
   - Titel en beschrijving van de request
   - Wie de request heeft gedaan
   - Media type (Movie/TV Show)
   - Request ID
   - Thumbnail afbeelding (indien beschikbaar)
3. Klik op **✅ Approve** om de request goed te keuren
4. Klik op **❌ Decline** om de request af te wijzen
5. Het bericht wordt automatisch bijgewerkt met het resultaat

### Troubleshooting

**Webhook wordt niet ontvangen:**
- Controleer of de webhook URL correct is geconfigureerd in Overseerr
- Controleer of Requestrr bereikbaar is vanaf de Overseerr server
- Check de Requestrr logs voor errors: `docker logs requestrr`

**Berichten verschijnen niet in Discord:**
- Controleer of `ModeratorChannels` correct is geconfigureerd
- Controleer of het Channel ID correct is
- Controleer of de bot de juiste permissions heeft in het kanaal

**Knoppen werken niet:**
- Controleer of de gebruiker de juiste permissions heeft (Administrator of Manage Messages)
- Check de Requestrr logs voor errors
- Controleer of de Overseerr API key correct is geconfigureerd

**Approve/Decline faalt:**
- Controleer of de Overseerr API key correct is en de juiste permissions heeft
- Controleer de Overseerr logs voor errors
- Controleer of de request nog bestaat in Overseerr

## API Endpoints

### Webhook Endpoint
```
POST /api/webhooks/overseerr
```

Accepteert Overseerr webhook payloads en verwerkt `MEDIA_PENDING` notifications.

**Request Body:**
```json
{
  "notification_type": "MEDIA_PENDING",
  "subject": "New Request",
  "message": "User has requested Movie Name",
  "image": "https://image.tmdb.org/...",
  "media": {
    "media_type": "movie",
    "tmdbId": 12345,
    "status": 2
  },
  "request": {
    "request_id": 123,
    "requestedBy_username": "username",
    "requestedBy_email": "user@example.com"
  }
}
```

## Technische Details

### Bestanden
- `Requestrr.WebApi/Controllers/Webhooks/OverseerrWebhookController.cs` - Webhook endpoint controller
- `Requestrr.WebApi/RequestrrBot/Webhooks/OverseerrWebhookService.cs` - Discord message service
- `Requestrr.WebApi/RequestrrBot/Webhooks/OverseerrModerationService.cs` - Overseerr API integration
- `Requestrr.WebApi/RequestrrBot/ChatClients/Discord/DiscordSettings.cs` - Settings model (updated)
- `Requestrr.WebApi/RequestrrBot/ChatBot.cs` - Button interaction handler (updated)

### Overseerr API Endpoints Gebruikt
- `POST /api/v1/request/{requestId}/approve` - Approve een request
- `POST /api/v1/request/{requestId}/decline` - Decline een request
- `GET /api/v1/request/{requestId}` - Haal request details op

## Veiligheid

- De webhook endpoint is publiekelijk toegankelijk (geen authenticatie vereist)
- Alleen gebruikers met moderator permissions kunnen requests goedkeuren/afwijzen
- Alle acties worden gelogd in de Requestrr logs

## Toekomstige Verbeteringen

Mogelijke uitbreidingen:
- Configureerbare moderator roles (naast permissions)
- Reden toevoegen bij decline
- Notificaties naar de requester
- Statistieken over goedgekeurde/afgewezen requests
- Webhook authenticatie/signing
