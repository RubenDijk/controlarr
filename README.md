## Running with Docker

This project provides a Docker setup for the `Requestrr.WebApi` service using .NET 7.0. The default configuration exposes the application on port `5000`.

### Requirements
- Docker and Docker Compose installed
- .NET SDK/ASP.NET Core Runtime version: **7.0** (handled by the Dockerfile)

### Build and Run

To build and run the service:

```sh
docker compose up --build
```

This will build the image using the provided Dockerfile and start the `csharp-requestrr_webapi` container.

### Ports
- **5000**: The application is available on `http://localhost:5000` by default.

### Environment Variables
- The container sets `ASPNETCORE_URLS` to `http://+:5000` by default.
- No additional environment variables are required for a basic setup. If you need to provide custom environment variables, you can use a `.env` file and uncomment the `env_file` line in the `docker-compose.yml`.

### Special Configuration
- No external databases or caches are required.
- No persistent volumes are configured by default.
- The container runs as a non-root user for improved security.

If you need to customize the configuration, refer to the `config/` directory and the `SettingsTemplate.json` file for available options.
