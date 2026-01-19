# Controlarr

A Discord bot for requesting media content via Sonarr, Radarr, Lidarr, Overseerr, and Ombi. Fork of Requestrr with additional features including Overseerr webhook moderation.

## Features

- 🎬 Request movies via Radarr or Overseerr
- 📺 Request TV shows via Sonarr or Overseerr
- 🎵 Request music via Lidarr
- 🔔 Notifications when content is available
- ✅ **NEW:** Overseerr webhook moderation - approve/decline requests directly from Discord
- 🌐 Web-based configuration interface

---

## Running with Docker

### Quick Start

```bash
docker compose up -d --build
```

Access the web interface at: **http://localhost:4545**

### Docker Compose

```yaml
services:
  controlarr:
    image: ghcr.io/rubendijk/controlarr:latest
    container_name: controlarr
    restart: unless-stopped
    ports:
      - "4545:4545"
    volumes:
      - ./config:/app/config
    environment:
      - TZ=Europe/Amsterdam
```

---

## Unraid Installation

### Option 1: Docker Compose (Recommended)

1. Install **Compose Manager** plugin from Community Applications
2. Create folder: `/mnt/user/appdata/controlarr/`
3. Clone the repository:
   ```bash
   cd /mnt/user/appdata/controlarr
   git clone https://github.com/RubenDijk/controlarr.git source
   ```
4. Create `docker-compose.yml`:
   ```yaml
   services:
     controlarr:
       build:
         context: ./source
         dockerfile: ./Requestrr.WebApi/dockerfile
       container_name: controlarr
       restart: unless-stopped
       ports:
         - "4545:4545"
       volumes:
         - /mnt/user/appdata/controlarr/config:/app/config
       environment:
         - TZ=Europe/Amsterdam
   ```
5. Start via Compose Manager

### Option 2: Manual Docker Container

```bash
# Build the image
cd /mnt/user/appdata/controlarr/source
docker build -t controlarr -f ./Requestrr.WebApi/dockerfile .

# Run the container
docker run -d \
  --name controlarr \
  --restart unless-stopped \
  -p 4545:4545 \
  -v /mnt/user/appdata/controlarr/config:/app/config \
  -e TZ=Europe/Amsterdam \
  controlarr
```

---

## Configuration

### First Run

1. Open **http://your-server-ip:4545**
2. Create an admin account
3. Configure your Discord bot:
   - Application ID
   - Bot Token
4. Configure download clients (Radarr, Sonarr, Overseerr, etc.)

### Overseerr Webhook Moderation

To enable approve/decline buttons for Overseerr requests:

1. In Controlarr, go to **Chat Clients** → **Overseerr Moderation Settings**
2. Add your moderator channel ID(s)
3. In Overseerr, go to **Settings** → **Notifications** → **Webhook**
4. Add webhook URL: `http://your-controlarr-ip:4545/api/webhooks/overseerr`
5. Enable **Media Pending** notification type

See [OVERSEERR_WEBHOOK_SETUP.md](OVERSEERR_WEBHOOK_SETUP.md) for detailed instructions.

---

## Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `TZ` | `Europe/Amsterdam` | Timezone |
| `ASPNETCORE_URLS` | `http://+:4545` | Listen URL |
| `REQUESTRR_PORT` | `4545` | Application port |

---

## Ports

| Port | Description |
|------|-------------|
| 4545 | Web interface and API |

---

## Volumes

| Path | Description |
|------|-------------|
| `/app/config` | Configuration files (settings.json, notifications.json) |

---

## Building from Source

### Requirements
- .NET SDK 6.0
- Node.js 18+

### Build

```bash
cd Requestrr.WebApi
dotnet publish -c Release -o ../publish
```

---

## Support

- GitHub Issues: [https://github.com/RubenDijk/controlarr/issues](https://github.com/RubenDijk/controlarr/issues)

---

## License

MIT License - See [LICENSE](LICENSE) for details.
