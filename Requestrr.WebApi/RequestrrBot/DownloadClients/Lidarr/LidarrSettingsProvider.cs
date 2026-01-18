using Requestrr.WebApi.RequestrrBot.DownloadClients.Radarr;
using System;

namespace Requestrr.WebApi.RequestrrBot.DownloadClients.Lidarr
{
    public class LidarrSettingsProvider
    {
        public LidarrSettings Provider()
        {
            dynamic settings = SettingsFile.Read();

            return new LidarrSettings
            {
                Hostname = settings.DownloadClients.Lidarr.Hostname,
                BaseUrl = settings.DownloadClients.Lidarr.BaseUrl,
                Port = (int)settings.DownloadClients.Lidarr.Port,
                ApiKey = settings.DownloadClients.Lidarr.ApiKey,
                Categories = settings.DownloadClients.Lidarr.Categories.ToObject<LidarrCategory[]>(),
                SearchNewRequests = settings.DownloadClients.Lidarr.SearchNewRequests,
                MonitorNewRequests = settings.DownloadClients.Lidarr.MonitorNewRequests,
                UseSSL = (bool)settings.DownloadClients.Lidarr.UseSSL,
                Version = settings.DownloadClients.Lidarr.Version
            };
        }
    }
}
