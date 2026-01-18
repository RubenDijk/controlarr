using Microsoft.Extensions.Primitives;
using Requestrr.WebApi.RequestrrBot.Music;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.Notifications.Music
{
    public interface IMusicNotifier
    {
        Task<HashSet<string>> NotifyArtistAsync(IReadOnlyCollection<string> userIds, MusicArtist musicArtist, CancellationToken token);
    }
}
