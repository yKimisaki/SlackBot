using Microsoft.Extensions.Configuration;

namespace SlackBot.Api
{
    internal static class AppSettings
    {
        public static IConfiguration? Configuration { get; set; }

        public static string AccessToken
        {
            get
            {
                return Configuration?.GetValue<string>("SlackOAuthAccessToken") ?? "";
            }
        }

        public static string BotName
        {
            get 
            {
                return Configuration?.GetValue<string>("SlackBotName") ?? ""; 
            }
        }
    }
}
