using Requestrr.WebApi.RequestrrBot.Movies;

namespace Requestrr.WebApi.RequestrrBot.Music
{
    public class MusicRequest
    {
        public int CategoryId { get; }
        public MusicUserRequester User { get; }

        public MusicRequest(MusicUserRequester user, int categoryId)
        {
            User = user;
            CategoryId = categoryId;
        }
    }
}
