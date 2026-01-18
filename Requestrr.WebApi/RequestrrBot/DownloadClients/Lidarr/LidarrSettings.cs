
using System;

namespace Requestrr.WebApi.RequestrrBot.DownloadClients.Lidarr
{
    public class LidarrSettings
    {
        public string Hostname { get; set; }
        public int Port { get; set; }
        public string ApiKey { get; set; }
        public string BaseUrl { get; set; }
        public LidarrCategory[] Categories { get; set; } = Array.Empty<LidarrCategory>();
        public bool SearchNewRequests { get; set; }
        public bool MonitorNewRequests { get; set; }
        public bool UseSSL { get; set; }
        public string Version { get; set; }
    }

    public class LidarrCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProfileId { get; set; }
        public int MetadataProfileId { get; set; }
        public string RootFolder { get; set; }
        public int[] Tags { get; set; } = Array.Empty<int>();
    }
}
