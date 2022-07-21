using System.Threading.Tasks;
using TwitterSentimentAnalysis.Model.Twitter;

namespace TwitterSentimentAnalysis.Services.Abstractions
{
    public interface ITwitterService
    {
        public Task<TweetData[]> FindTweetsByHashtag(string[] hashtags, int maxTweets);
    }
}
