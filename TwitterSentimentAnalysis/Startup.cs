using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TwitterSentimentAnalysis;
using TwitterSentimentAnalysis.Services;
using TwitterSentimentAnalysis.Services.Abstractions;

[assembly: WebJobsStartup(typeof(Startup))]

namespace TwitterSentimentAnalysis
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.Services.AddHttpClient();
            builder.Services.AddLogging();
            builder.Services.AddScoped<ILuisService, LuisService>();
            builder.Services.AddScoped<ITwitterService, TwitterService>();
        }
    }
}
