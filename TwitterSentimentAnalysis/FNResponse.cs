using System.Collections.Generic;
using TwitterSentimentAnalysis.Model.Twitter;

namespace TwitterSentimentAnalysis
{
    internal class FNResponse
    {
        public TweetData[] MostPopularTweets { get; set; } 
        public int NumberOfTweetsFound { get; set; }
        public Dictionary<TweetSentiment, int> TweetSentimentAnalysis { get; set; } 
    }
}
