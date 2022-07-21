using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using TwitterSentimentAnalysis.Services.Abstractions;
using TwitterSentimentAnalysis.Model.Twitter;

namespace TwitterSentimentAnalysis.Services
{
    public class TwitterService : ITwitterService
    {
        readonly IHttpClientFactory _httpClientFactory;

        readonly string bearerToken;
        readonly string baseUrl;

        public TwitterService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;

            bearerToken = Environment.GetEnvironmentVariable("TwitterBearerToken");
            baseUrl = Environment.GetEnvironmentVariable("TwitterBaseUrl");
        }

        public async Task<TweetData[]> FindTweetsByHashtag(string[] hashtags, int maxTweets)
        {
            string paginationToken = null;
            string hashtagQuery = GetHashtagQuery(hashtags);

            List<TweetData> tweetsFound = new();

            while (tweetsFound.Count < maxTweets && (paginationToken != null || tweetsFound.Count == 0))
            {
                var twitterSearchResult = await GetNextSetOfTweets(paginationToken, hashtagQuery);

                if (twitterSearchResult != null)
                {
                    paginationToken = twitterSearchResult.meta.next_token;

                    if (tweetsFound.Count + twitterSearchResult.meta.result_count > maxTweets)
                    {
                        var diff = maxTweets - tweetsFound.Count;

                        for (int i = 0; i < diff; i++)
                        {
                            tweetsFound.Add(twitterSearchResult.data[i]);
                        }
                    }
                    else
                    {
                        tweetsFound.AddRange(twitterSearchResult.data);
                    }
                }
                else
                {
                    return tweetsFound.ToArray();
                }

            }

            return tweetsFound.ToArray();
        }

        string GetHashtagQuery(string[] hashTags)
        {
            string query = string.Empty;

            foreach (string hashTag in hashTags)
            {
                var tag = hashTag.StartsWith('#') ?  hashTag : $"#{hashTag}";
                query = string.IsNullOrEmpty(query) ? tag : query + $" {tag}";
            }

            return HttpUtility.UrlEncode(query);
        }

        async Task<TwitterSearchResult> GetNextSetOfTweets(string paginationToken, string query)
        {
            string paginationSubQuery = string.IsNullOrEmpty(paginationToken) ? string.Empty : $"&next_token={paginationToken}";

            string url = $"{baseUrl}?query={query}&tweet.fields=public_metrics{paginationSubQuery}";

            var message = new HttpRequestMessage(HttpMethod.Get, url)
            {
                Headers =
                {
                    { "Authorization", $"Bearer {bearerToken}" }
                }
            };

            using (var client = _httpClientFactory.CreateClient())
            {
                var response = await client.SendAsync(message);

                if (response.IsSuccessStatusCode)
                {
                    using var contentStream = await response.Content.ReadAsStreamAsync();

                    return await JsonSerializer.DeserializeAsync<TwitterSearchResult>(contentStream);
                }
            }

            return null;
        }
    }
}