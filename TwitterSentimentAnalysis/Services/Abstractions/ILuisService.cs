using System.Collections.Generic;
using System.Threading.Tasks;
using TwitterSentimentAnalysis.Model.Twitter;

namespace TwitterSentimentAnalysis.Services.Abstractions
{
    public interface ILuisService
    {
        public Task<Dictionary<TweetSentiment, int>> GetSentimentAnalysisOnTweets(TweetData[] tweets);
    }
}
