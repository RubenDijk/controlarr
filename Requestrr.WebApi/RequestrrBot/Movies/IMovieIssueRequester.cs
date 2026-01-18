using System.Threading.Tasks;


namespace Requestrr.WebApi.RequestrrBot.Movies
{
    public interface IMovieIssueRequester
    {
        Task<bool> SubmitMovieIssueAsync(MovieRequest request, int theMovieDbId, string issueValue, string issueDescription);
    }
}
