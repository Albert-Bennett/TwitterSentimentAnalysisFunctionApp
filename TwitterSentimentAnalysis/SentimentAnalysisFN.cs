using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TwitterSentimentAnalysis.Services.Abstractions;

namespace TwitterSentimentAnalysis
{
    public class SentimentAnalysisFN
    {
        private readonly ITwitterService _twitterService;
        public readonly ILuisService _luisService;

        public SentimentAnalysisFN(ITwitterService twitterService, ILuisService luisService)
        {
            _twitterService = twitterService;
            _luisService = luisService;
        }

        [FunctionName("SentimentAnalysisFN")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "max_tweets", In = ParameterLocation.Query, Required = true, Type = typeof(int), Description = "The parameter defining how many popular tweets to return")]
        [OpenApiParameter(name: "hashtags", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The parameter defining a comma delimited set of hastags without the hash symbol before the text")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req)
        {
            int maxTweets = 0;

            if (!int.TryParse(req.Query["max_tweets"], out maxTweets))
            {
                return new BadRequestObjectResult("max_tweets must be a number greater than 0 and less than 100");
            }

            if (maxTweets <= 0)
            {
                return new BadRequestObjectResult("max_tweets must be greater than 0");
            }
            else if (maxTweets > 100)
            {
                return new BadRequestObjectResult("max_tweets must be less than 100");
            }

            string hashtagQueryParam = req.Query["hashtags"];

            string[] hashtags = hashtagQueryParam.Split('\u002C');

            if (hashtags == null || hashtags.Length == 0)
            {
                return new BadRequestObjectResult("You must include hashtags to search in the request body");
            }

            var foundTweets = await _twitterService.FindTweetsByHashtag(hashtags, maxTweets);

            if (foundTweets != null)
            {
                var maxNumberOfPopularTweets = int.Parse(Environment.GetEnvironmentVariable("MaxNumberOfPopularTweets"));
                var mostPopularTweets = foundTweets.OrderByDescending(x => x.public_metrics.GetPopularity()).Take(maxNumberOfPopularTweets).ToArray();

                var fnResponse = new FNResponse
                {
                    MostPopularTweets = mostPopularTweets,
                    NumberOfTweetsFound = foundTweets.Length,
                    TweetSentimentAnalysis = await _luisService.GetSentimentAnalysisOnTweets(foundTweets)
                };

                return new OkObjectResult(fnResponse);
            }

            return new BadRequestObjectResult($"No tweets found searching for the following hashtags: {hashtags}.");
        }
    }
}