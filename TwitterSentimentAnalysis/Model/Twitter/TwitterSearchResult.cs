namespace TwitterSentimentAnalysis.Model.Twitter
{
    public class TwitterSearchResult
    {
        public TweetData[] data { get; set; }
        public TwitterSearchResultMetaData meta { get; set; }
    }
}
