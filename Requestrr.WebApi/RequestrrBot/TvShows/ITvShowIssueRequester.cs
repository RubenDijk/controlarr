
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.TvShows
{
    public interface ITvShowIssueRequester
    {
        Task<bool> SubmitTvShowIssueAsync(TvShowRequest request, int theTvDbId, string issueValue, string issueDescription);
    }
}