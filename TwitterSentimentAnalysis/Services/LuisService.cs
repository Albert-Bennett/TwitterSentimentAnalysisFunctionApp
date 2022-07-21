using System.Collections.Generic;
using System.Threading.Tasks;
using TwitterSentimentAnalysis.Services.Abstractions;
using TwitterSentimentAnalysis.Model.Twitter;
using System;
using System.Net.Http;
using System.Web;
using TwitterSentimentAnalysis.Model.Luis;
using System.Text.Json;

namespace TwitterSentimentAnalysis.Services
{
    public class LuisService : ILuisService
    {
        readonly IHttpClientFactory _httpClientFactory;

        readonly string subscriptionKey;
        readonly string appId;
        readonly string baseUrl;
        readonly string endpoint;

        public LuisService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;

            subscriptionKey = Environment.GetEnvironmentVariable("SubscriptionKey");
            appId = Environment.GetEnvironmentVariable("AppId");
            baseUrl = Environment.GetEnvironmentVariable("LuisBaseUrl");
            endpoint = Environment.GetEnvironmentVariable("LuisEndpoint");
        }

        public async Task<Dictionary<TweetSentiment, int>> GetSentimentAnalysisOnTweets(TweetData[] tweets)
        {
            Dictionary<TweetSentiment, int> sentimentAnalysis = new Dictionary<TweetSentiment, int>();

            foreach (TweetData tweet in tweets)
            {
                var analysis = await GetSentimentAnalaysis(tweet.text);

                if (sentimentAnalysis.ContainsKey(analysis))
                {
                    sentimentAnalysis[analysis]++;
                }
                else
                {
                    sentimentAnalysis.Add(analysis, 1);
                }
            }

            return sentimentAnalysis;
        }

        async Task<TweetSentiment> GetSentimentAnalaysis(string text)
        {
            string url = $"{baseUrl}{appId}{endpoint}?subscription-key={subscriptionKey}&query={HttpUtility.UrlEncode(text)}";

            using (var client = _httpClientFactory.CreateClient())
            {
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    using var contentStream = await response.Content.ReadAsStreamAsync();

                    var result = await JsonSerializer.DeserializeAsync<LuisQueryResult>(contentStream);

                    switch (result.prediction.sentiment.label)
                    {
                        case LuisConstants.NegativeResult:
                            return TweetSentiment.Negative;

                        case LuisConstants.PositiveResult:
                            return TweetSentiment.Positive;

                        default:
                            return TweetSentiment.Neutral;
                    }
                }
            }

            return TweetSentiment.InConclusive;
        }
    }
}