using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SlackBot.Api.Commands.Core;
using System.Collections.Generic;
using System.Net.Http;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SlackBot.Api
{
    [ApiController]
    [Route("api/command")]
    public class CommandController : Controller
    {
        private readonly ILogger logger;

        private const string BotName = "";
        private const string BotToken = "";

        public CommandController(ILogger<CommandController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        public string Info([FromForm]string text, [FromForm]string channel_id, [FromForm]string user_name)
        {
            logger.LogInformation($"Command: text: {text}, channel: {channel_id}, username: {user_name}");

            try
            {
                var result = CommandExecutor.ExecuteAsync(user_name, channel_id, text).Result;
                using (var httpClient = new HttpClient())
                {
                    var response = new Dictionary<string, string>
                    {
                        { "token",  BotToken },
                        { "channel", channel_id },
                        { "text", $"<@{user_name}> {result}" },
                        { "reply_broadcast", result.IsBroadcast ? "true" : "false" },
                        { "username", BotName},
                    };
                    var content = new FormUrlEncodedContent(response);
                    httpClient.PostAsync("https://slack.com/api/chat.postMessage", content).Wait();
                }
            }
            catch (CommandNotFoundException)
            {
                logger.LogWarning($"Command is not found.");
            }
            catch
            {
                throw;
            }

            return "";
        }
    }
}
