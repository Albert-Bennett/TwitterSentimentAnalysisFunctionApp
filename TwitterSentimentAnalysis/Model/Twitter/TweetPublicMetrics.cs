namespace TwitterSentimentAnalysis.Model.Twitter
{
    public class TweetPublicMetrics
    {
        public int retweet_count { get; set; }
        public int reply_count { get; set; }
        public int like_count { get; set; }
        public int quote_count { get; set; }

        /// <summary>
        /// This method is just going to add up all of the public metrics to get an arbitrary popularity metric
        /// </summary>
        /// <returns></returns>
        public int GetPopularity()
        {
            return retweet_count + reply_count + like_count + quote_count;
        }
    }
}
