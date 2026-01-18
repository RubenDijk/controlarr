using System.Collections.Generic;
using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.Music
{
    public interface IMusicUserInterface
    {
        Task ShowMusicArtistSelection(MusicRequest request, IReadOnlyList<MusicArtist> music);
        Task WarnNoMusicArtistFoundAsync(string musicName);

        Task DisplayMusicArtistDetailsAsync(MusicRequest request, MusicArtist music);
        Task DisplayArtistRequestDeniedAsync(MusicArtist music);
        Task DisplayArtistRequestSuccessAsync(MusicArtist music);

        Task WarnMusicArtistAlreadyAvailableAsync(MusicArtist music);

        Task WarnMusicArtistUnavailableAndAlreadyHasNotificationAsync(MusicArtist music);
        Task AskForNotificationArtistRequestAsync(MusicArtist music);
        Task DisplayNotificationArtistSuccessAsync(MusicArtist music);
    }
}
